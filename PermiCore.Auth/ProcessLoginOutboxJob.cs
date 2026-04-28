using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PermiCore.Auth.Database;
using PermiCore.Auth.Entity;
using System.Text.Json;

namespace PermiCore.Auth
{
    public class ProcessLoginOutboxJob(AuthDbContext authDbContext, HybridCache hybridCache)
    {
        public async Task Execute()
        {
            var queryableOutbox = authDbContext.Outbox
                .Where(y => !y.IsDeleted && y.Type == new LoginUser().GetType().Name);

            var loginUsers = queryableOutbox
                .AsEnumerable()
                .Select(y => JsonSerializer.Deserialize<LoginUser>(y.Payload))
                .Where(x => x != null)
                .Select(y => y!);


            var cachedUserDto = await Task.WhenAll(loginUsers.Select(async y => new
            {
                y.Id,
                Permissions = await authDbContext.UserPermissions.AsNoTrackingWithIdentityResolution().Include(x => x.Permission).Where(y => y.UserId == y.UserId).Select(y => y.Permission.Permission).ToListAsync()
            }));


            var tasks = cachedUserDto.Select(y => hybridCache.SetAsync(
                 key: $"AuthServer:{y.Id}",
                 value: y).AsTask());


            await Task.WhenAll(tasks);


            using var transaction = await authDbContext.Database.BeginTransactionAsync();
            try
            {
                authDbContext.LoginUser.AddRange(loginUsers);
                await authDbContext.SaveChangesAsync();
                await queryableOutbox.ExecuteUpdateAsync(setProp => setProp.SetProperty(p => p.IsDeleted, true));
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
