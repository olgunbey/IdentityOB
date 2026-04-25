using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.Entity;
namespace YarpExample.Gateway
{
    public class ProcessInboxJob(GatewayDbContext gatewayDbContext, HybridCache hybridCache)
    {
        public async Task Execute()
        {
            var processedOutboxData = gatewayDbContext.Outbox.Where(x => !x.IsDeleted &&
            !gatewayDbContext.Inbox.Select(y => y.IdempotencyId).Contains(x.IdempotencyId));

            if (!processedOutboxData.Any())
                return;

            var addedInbox = processedOutboxData
                 .Select(y => new Inbox()
                 {
                     IdempotencyId = y.IdempotencyId,
                     Payload = y.Payload,
                     WriteDateTime = DateTime.UtcNow,
                     ProcessedDateTime = null,
                     InboxOutboxType = y.InboxOutboxType
                 });

            await using var transaction = await gatewayDbContext.Database.BeginTransactionAsync();
            try
            {
                await gatewayDbContext.Inbox.AddRangeAsync(addedInbox);
                await gatewayDbContext.SaveChangesAsync();
                await processedOutboxData.ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, true));

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            await transaction.DisposeAsync();

            var inboxItems = gatewayDbContext.Inbox
                .Where(x => x.ProcessedDateTime == null
            && x.InboxOutboxType == new ServicesPermissions().GetType().Name);

            var permissions = inboxItems
                .AsEnumerable()
                .Select(x => JsonSerializer.Deserialize<ServicesPermissions>(x.Payload))
                .ToList();

            if (!permissions.Any())
                return;

            await using var transaction2 = await gatewayDbContext.Database.BeginTransactionAsync();
            try
            {
                gatewayDbContext.ServicesPermissions.AddRange(permissions);
                await gatewayDbContext.SaveChangesAsync();
                await inboxItems.ExecuteUpdateAsync(x => x.SetProperty(p => p.ProcessedDateTime, DateTime.UtcNow));
                await transaction2.CommitAsync();
            }
            catch (Exception)
            {
                await transaction2.RollbackAsync();
                throw;
            }
            await transaction2.DisposeAsync();

            var rawData = await gatewayDbContext.ServicesPermissions
                .AsNoTracking()
                .Where(y => permissions.Select(x => x.ServiceId).Contains(y.ServiceId))
                .GroupBy(x => x.ServiceId)
                .Select(y => new
                {
                    y.Key,
                    Permissions = y.Select(a => a.Permission.Permission).ToList()
                })
                .ToListAsync();

            var tasks = rawData.Select(x => hybridCache.SetAsync(
                key: $"service-permissions:{x.Key}",
                value: x.Permissions,
                options: new HybridCacheEntryOptions()
                {
                    LocalCacheExpiration = TimeSpan.FromDays(100),
                    Expiration = TimeSpan.FromDays(100)
                }).AsTask());

            await Task.WhenAll(tasks);

        }
    }
}
