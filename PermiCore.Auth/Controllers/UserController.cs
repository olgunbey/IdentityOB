using Microsoft.AspNetCore.Mvc;
using PermiCore.Auth.Dtos.Request;
using PermiCore.Auth.Dtos.Response;

namespace PermiCore.Auth.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public IActionResult Login(LoginRequestDto loginRequestDto)
        {
            if (loginRequestDto != null) //username password kontrolü yapılacak
            {
                var userKey = new LoginResponseDto()
                {
                    UserKey = Guid.NewGuid().ToString()
                };
                //userKey redise kaydedilecek
                return Ok(userKey);
            }
            return BadRequest();
        }
    }
}
