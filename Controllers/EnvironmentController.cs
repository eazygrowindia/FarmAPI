using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        [HttpGet("env")]
        public IActionResult GetEnvironment(IWebHostEnvironment env)
        {
            return Ok(new { Environment = env.EnvironmentName, IsProduction = env.IsProduction() });
        }

        [HttpGet("settings")]
        public IActionResult GetSettings(IConfiguration config)
        {
            return Ok(new { Config = config.AsEnumerable().ToList()});
        }
    }
}
