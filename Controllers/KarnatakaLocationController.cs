using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO;

namespace EasyGrow.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/[controller]")]
    //[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger unless admin
    public class KarnatakaLocationController : ControllerBase
    {
        private readonly KarnatakaImportService _importService;

        public KarnatakaLocationController(KarnatakaImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExternalService()
        {
            var count = await _importService.HarvestKarnatakaHierarchyAsync();
            return Ok(new { imported = count });
        }

    }
}
