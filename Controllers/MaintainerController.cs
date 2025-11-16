using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintainerController : ControllerBase
    {
        private readonly MaintainerService _maintainerService;

        public MaintainerController(MaintainerService maintainerService) =>
            _maintainerService = maintainerService;

        [HttpGet]
        public async Task<List<Maintainer>> Get() =>
            await _maintainerService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Maintainer>> Get(string id)
        {
            var maintainer = await _maintainerService.GetAsync(id);

            if (maintainer is null)
            {
                return NotFound();
            }

            return maintainer;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Maintainer newMaintainer)
        {
            await _maintainerService.CreateAsync(newMaintainer);

            return CreatedAtAction(nameof(Get), new { id = newMaintainer.Id }, newMaintainer);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Maintainer updatedMaintainer)
        {
            var maintainer = await _maintainerService.GetAsync(id);

            if (maintainer is null)
            {
                return NotFound();
            }

            updatedMaintainer.Id = maintainer.Id;

            await _maintainerService.UpdateAsync(id, updatedMaintainer);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var maintainer = await _maintainerService.GetAsync(id);

            if (maintainer is null)
            {
                return NotFound();
            }

            await _maintainerService.RemoveAsync(id);

            return NoContent();
        }
    }
}