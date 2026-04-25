using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Transforms;
using YarpExample.Gateway.Database;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.Service;

namespace YarpExample.Gateway.RequestTransforms
{
    public class AuthorizationRequestTransform(HybridCache hybridCache, GatewayDbContext gatewayDbContext, GatewayService gatewayService) : RequestTransform
    {
        public override async ValueTask ApplyAsync(RequestTransformContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Request.Headers.TryGetValue("X-User-Id", out StringValues val);
            if (!int.TryParse(val.ToString(), out int userId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var getAllAuthUser = await hybridCache.GetOrCreateAsync<AuthRedisResponseDto?>(
                key: $"AuthServer:{userId}",
                factory: async (ct) => null);

            if (getAllAuthUser == null) //Ya kullanıcı ok ya da lifetime süresi doldu. 
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var path = httpContext.Request.Path.ToString().ToLower();

            var requestService = await gatewayDbContext.Services.SingleAsync(x => x.RequestPath == path);

            var servicePermissions = await hybridCache.GetOrCreateAsync<ServicePermissionRedisCacheDto>(
                  key: $"service-permissions:{requestService.Id}",
                  factory: (ct) =>
                  {
                      var servicePermissions = gatewayDbContext.ServicesPermissions.
                          AsNoTrackingWithIdentityResolution().
                          Include(y => y.Service).
                          Include(y => y.Permission).
                          Where(x => x.Service.RequestPath == path).AsEnumerable();
                      var servicePermissionRedisCacheDto = new ServicePermissionRedisCacheDto()
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
