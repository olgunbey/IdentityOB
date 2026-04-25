using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Transforms;

namespace YarpExample.Gateway.RequestTransforms
{
    public class AuthorizationRequestTransform(HybridCache redisService) : RequestTransform
    {
        public override async ValueTask ApplyAsync(RequestTransformContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Request.Headers.TryGetValue("X-User-Key", out StringValues val);
            string value = val.ToString();
            if (string.IsNullOrWhiteSpace(val))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }


            //var getAllAuthUser = await redisService.ReadRedis<AuthRedisResponseDto>("AuthServer");

            //if (getAllAuthUser == null || !getAllAuthUser.Any())
            //{
            //    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    return;
            //}

            //var hasAuthUser = getAllAuthUser.FirstOrDefault(x => x.UserKey == value);

            //if (hasAuthUser == null) //Bu kullanıcı rediste yok
            //{
            //    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    return;
            //}
            //if (hasAuthUser != null && hasAuthUser.LifeTime < DateTime.UtcNow) //Bu kullanıcı rediste var ama süresi dolmuş
            //{
            //    await redisService.UpdateUserRedis(value); //Burada kullanıcının süresini güncelliyoruz
            //}
            //var path = httpContext.Request.Path.ToString().ToLower();


            //var servicePermissions = gatewayDbContext.ServicesPermissions.
            //     AsNoTrackingWithIdentityResolution().
            //     Include(y => y.Service).
            //     Include(y => y.Permission).
            //     Where(x => x.Service.RequestPath == path).AsEnumerable();
            //if (!await permissionService.SearchPermission(servicePermissions, hasAuthUser!.Permissions))
            //{
            //    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            //    return;
            //}

        }
    }
}
