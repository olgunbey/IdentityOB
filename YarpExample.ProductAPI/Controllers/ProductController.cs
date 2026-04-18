using Microsoft.AspNetCore.Mvc;
using YarpExample.Shared;

namespace YarpExample.ProductAPI.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [OBAuth(new[] { "Perm1", "Perm2","Perm3" }, PermissionMatchType.Any)]
        [HttpGet]
        public IActionResult GetAllProduct()
        {
            var products = new List<string>() { "Product1", "Product2", "Product3" };
            return Ok(products);
        }
    }
}
