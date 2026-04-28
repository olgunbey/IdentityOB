using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos.Response;
using YarpExample.Gateway.Service;

namespace YarpExample.Gateway.RequestTransforms
{
    public class AuthorizationRequestTransform(HybridCache hybridCache, GatewayDbContext gatewayDbContext, GatewayService gatewayService, HttpClient httpClient) : RequestTransform
    {
        public override async ValueTask ApplyAsync(RequestTransformContext context)
        {
            var httpContext = context.HttpContext;
            var test = httpContext.Response.Cookies;
            var userKey = httpContext.Request.Cookies["UserKey"];
            if (!int.TryParse(userKey, out int userId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var getAllAuthUser = await hybridCache.GetOrCreateAsync<AuthRedisResponseDto?>(
                key: $"AuthServer:{userId}",
                factory: async (ct) =>
                {
                    var allUser = await httpClient.GetFromJsonAsync<List<AuthRedisResponseDto?>>($"/user/GetAllUserPermissions");

                    if (allUser == null) return null;

                    var validUsers = allUser
                        .Where(x => x != null)
                        .Select(x => x!)
                        .ToList();

                    var cacheTasks = validUsers
                      .Select(user => hybridCache.SetAsync(
                          key: $"AuthServer:{user.UserId}",
                          value: user,
                          cancellationToken: ct).AsTask());

                    await Task.WhenAll(cacheTasks);

                    return allUser.SingleOrDefault(x => x.UserId == userId);

                });

            if (getAllAuthUser == null) //Ya kullanıcı ok ya da lifetime süresi doldu. 
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var path = httpContext.Request.Path.ToString().ToLower();


            var servicePermissions = await gatewayDbContext.Services
                .Include(y => y.ServicesPermissions)
                .ThenInclude(y => y.Permission)
                .FirstOrDefaultAsync(y => y.RequestPath == path);

            if (servicePermissions == null) return;

            var permissions = servicePermissions.ServicesPermissions.Select(y => y.Permission.Permission).ToList();

            if (!await gatewayService.SearchPermission(permissions, getAllAuthUser.Permissions))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

        }
    }
}
