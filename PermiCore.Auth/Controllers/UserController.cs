using Microsoft.AspNetCore.Mvc;
using PermiCore.Auth.Dtos.Request;
using PermiCore.Auth.Dtos.Response;
using ServiceStack.Redis;

namespace PermiCore.Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController(IRedisClientAsync redisClientManagerAsync) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            int userID = 1; //veritabanından kullanıcı id'si çekilecek
            List<string> permissions = new List<string>() { "ChildPerm1", "Child2Perm1" };
            var userKey = new LoginResponseDto();
            if (loginRequestDto != null) //username password kontrolü yapıldıktan sonra
            {
                var authUserList = await redisClientManagerAsync.GetAsync<List<AuthRedisUserDto>>("AuthServer");
                if (authUserList != null)
                {
                    AuthRedisUserDto? authRedisUserDto = authUserList.SingleOrDefault(y => y.UserId == userID);

                    if (authRedisUserDto != null)
                    {
                        userKey.UserKey = authRedisUserDto.UserKey;
                        return Ok(userKey);
                    } 
                }
                else
                {
                    authUserList = new List<AuthRedisUserDto>();
                    userKey.UserKey = Guid.NewGuid().ToString();
                    authUserList.Add(new AuthRedisUserDto()
                    {
                        UserId = userID,
                        UserKey = userKey.UserKey,
                        Permissions = permissions,
                        LifeTime = DateTime.Now.AddDays(10)
                    });
                }
                
                await redisClientManagerAsync.SetAsync<List<AuthRedisUserDto>>("AuthServer", authUserList);
                return Ok(userKey);
            }
            return BadRequest();
        }
    }
}
