
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceStack.Redis;
using YarpExample.Shared.Dtos;

namespace YarpExample.Shared
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OBAuthAttribute(string[] Permissions, PermissionMatchType permissionMatchType) : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string userKey = context.HttpContext.Request.Headers["X-User-Key"].ToString();

            IRedisClientAsync redisClientAsync = new RedisManagerPool("127.0.0.1:6379").GetClientAsync().Result;
            var authUserList = await redisClientAsync.GetAsync<List<AuthRedisResponseDto>>("AuthServer");

            if (authUserList == null)
                throw new Exception("Redis boş!");

            var data = authUserList.SingleOrDefault(x => x.UserKey == userKey);
            if (data == null)
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
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
