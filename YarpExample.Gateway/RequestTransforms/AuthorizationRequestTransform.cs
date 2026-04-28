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

            var requestService = await gatewayDbContext.Services.SingleAsync(x => x.RequestPath == path);

            var servicePermissions = await hybridCache.GetOrCreateAsync<ServicePermissionRedisResponseDto>(
                  key: $"service-permissions:{requestService.Id}",
                  factory: (ct) =>
                  {
                      var servicePermissions = gatewayDbContext.ServicesPermissions.
                          AsNoTrackingWithIdentityResolution().
                          Include(y => y.Service).
                          Include(y => y.Permission).
                          Where(x => x.Service.RequestPath == path).AsEnumerable();
                      var servicePermissionRedisCacheDto = new ServicePermissionRedisResponseDto()
                      {
                          Permissions = servicePermissions.Select(y => y.Permission.Permission).ToList(),
                      };
                      return ValueTask.FromResult(servicePermissionRedisCacheDto);
                  });


            if (!await gatewayService.SearchPermission(servicePermissions, getAllAuthUser.Permissions))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

        }
    }
}
