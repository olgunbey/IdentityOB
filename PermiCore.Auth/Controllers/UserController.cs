using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using PermiCore.Auth.Dtos.Request;
using PermiCore.Auth.Service;
using System.Text.Json;

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

            var userGuidKey = Guid.NewGuid();

            string permissions = JsonSerializer.Serialize(getUser.UserPermissions.Select(y => y.Permission.Permission));
            await authService.LoginAsync(getUser.Id, permissions, userGuidKey);

            Response.Cookies.Append("UserKey", userGuidKey.ToString(), new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1),
                SameSite = SameSiteMode.Strict
            });
            return Ok(userGuidKey);
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
