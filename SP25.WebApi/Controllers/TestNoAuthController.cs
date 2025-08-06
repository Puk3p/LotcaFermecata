using Microsoft.AspNetCore.Mvc;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestNoAuthController : ControllerBase
    {
        [HttpGet("hello")]
        public IActionResult Hello() => Ok("world");
    }
}
