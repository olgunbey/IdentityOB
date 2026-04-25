using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using PermiCore.Auth.Dtos.Request;
using PermiCore.Auth.Dtos.Response;
using PermiCore.Auth.Service;

namespace PermiCore.Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController(HybridCache hybridCache, AuthService authService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            var userKey = new LoginResponseDto();

            var getUser = await authService.GetUserById(loginRequestDto.Username, loginRequestDto.Password);

            if (getUser == null)
                return BadRequest();

            AuthRedisUserDto authRedisUserDto = new AuthRedisUserDto();
            authRedisUserDto.UserId = getUser.Id;
            authRedisUserDto.Permissions = getUser.UserPermissions.Select(y => y.Permission).SelectMany(x => x.UserPermissions).Select(x => x.Permission.Permission).ToList();


            var cacheAuthUser = await hybridCache.GetOrCreateAsync(
                 key: $"AuthServer:{getUser.Id}",
                 factory: async (ct) => authRedisUserDto,
                 options: new HybridCacheEntryOptions
                 {
                     Expiration = TimeSpan.FromDays(10)
                 }
                 );

            return Ok(cacheAuthUser.UserKey);
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpRequestDto signUpRequestDto)
        {
            return Ok();
        }
    }
}
