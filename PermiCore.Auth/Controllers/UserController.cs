using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using PermiCore.Auth.Dtos.Request;
using PermiCore.Auth.Entity;
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


            await authService.SaveOutbox(
                 new Entity.Outbox()
                 {
                     Payload = JsonSerializer.Serialize(new LoginUser() { UserId = getUser.Id }),
                     Type = new LoginUser().GetType().Name,
                     WriteDateTime = DateTime.UtcNow,
                 });


            var userGuidKey = Guid.NewGuid().ToString();

            await hybridCache.SetAsync(
                 key: $"AuthServer:{getUser.Id}",
                 value: getUser.UserPermissions.Select(y => y.Permission).SelectMany(x => x.UserPermissions).Select(x => x.Permission.Permission).ToList(),
                 options: new HybridCacheEntryOptions()
                 {
                     Expiration = TimeSpan.FromDays(1),
                     LocalCacheExpiration = TimeSpan.FromMinutes(10)
                 });

            Response.Cookies.Append("UserKey", userGuidKey, new CookieOptions()
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
