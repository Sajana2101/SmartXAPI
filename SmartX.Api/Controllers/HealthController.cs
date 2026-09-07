using Microsoft.AspNetCore.Mvc;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "Online",
                application = "Smart-X API"
            });
        }
    }
}