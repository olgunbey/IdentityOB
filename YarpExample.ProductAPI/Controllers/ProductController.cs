using Microsoft.AspNetCore.Mvc;

namespace YarpExample.ProductAPI.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllProduct()
        {
            var products = new List<string>() { "Product1", "Product2", "Product3" };
            return Ok(products);
        }
    }
}
