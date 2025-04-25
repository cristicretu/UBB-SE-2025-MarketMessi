using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("ornaldo");
        }
    }
} 