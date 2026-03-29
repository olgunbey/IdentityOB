using Microsoft.AspNetCore.Mvc;
using YarpExample.Shared;

namespace YarpExample.ProductAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [OBAuth(new string[] { "Auth1", "Auth2" }, PermissionMatchType.Any)]
        public IActionResult GetAllProduct()
        {
            var products = new List<string>() { "Product1", "Product2", "Product3" };
            return Ok(products);
        }
    }
}
