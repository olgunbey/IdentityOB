using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using YarpExample.Gateway.Dtos;

namespace YarpExample.Gateway.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServiceController() : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddServicePermission([FromBody] AddServicePermissionRequestDto addServicePermissionRequestDto)
        {

            return Ok();
        }
    }
}
