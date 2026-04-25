using Microsoft.AspNetCore.Mvc;
using YarpExample.Gateway.Dtos;
using YarpExample.Gateway.Service;

namespace YarpExample.Gateway.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServiceController(GatewayService gatewayService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddServicePermission([FromBody] AddServicePermissionRequestDto addServicePermissionRequestDto)
        {
            await gatewayService.AddServicePermissionOutbox(addServicePermissionRequestDto);
            return NoContent();
        }
    }
}
