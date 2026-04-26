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
            var getUser = await authService.GetUserById(loginRequestDto.Username, loginRequestDto.Password);

            if (getUser == null)
                return Unauthorized();


            AuthRedisUserDto authRedisUserDto = new AuthRedisUserDto();
            authRedisUserDto.Permissions = getUser.UserPermissions.Select(y => y.Permission).SelectMany(x => x.UserPermissions).Select(x => x.Permission.Permission).ToList();

            await hybridCache.SetAsync(
                 key: $"AuthServer:{getUser.Id}",
                 value: authRedisUserDto,
                 options: new HybridCacheEntryOptions()
                 {
                     Expiration = TimeSpan.FromDays(1),
                     LocalCacheExpiration = TimeSpan.FromMinutes(10)
                 });

            Response.Cookies.Append("UserKey", getUser.Id.ToString(), new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                SameSite = SameSiteMode.Strict
            });
            return Ok(getUser.Id);
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpRequestDto signUpRequestDto)
        {
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> LogOut([FromQuery] int userId)
        {
            await hybridCache.RemoveAsync(key: $"AuthServer:{userId}");
            return NoContent();
        }
    }
}
