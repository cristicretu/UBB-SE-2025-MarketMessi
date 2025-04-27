using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
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