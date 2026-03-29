
using Microsoft.AspNetCore.Mvc.Filters;
using YarpExample.Shared.Dtos;
using YarpExample.Shared.Services;

namespace YarpExample.Shared
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OBAuthAttribute(string[] Permissions, PermissionMatchType permissionMatchType) : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            RedisService redisService = (RedisService)context.HttpContext.RequestServices.GetService(typeof(RedisService))!;

            string userKey = context.HttpContext.Request.Headers["X-User-Key"].ToString();

            redisService.ReadRedis(userKey, out AuthRedisResponseDto data);
            if (permissionMatchType.Equals(PermissionMatchType.All))
            {

                if (!Permissions.All(data.Permissions.Contains))
                {
                    context.HttpContext.Response.StatusCode = 403;
                    return;
                }
            }
            else if (permissionMatchType.Equals(PermissionMatchType.Any))
            {
                if (!Permissions.Any(data.Permissions.Contains))
                {
                    context.HttpContext.Response.StatusCode = 403;
                    return;
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
