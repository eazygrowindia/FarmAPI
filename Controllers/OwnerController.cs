using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly OwnerService _ownerService;

        public OwnerController(OwnerService ownerService) =>
            _ownerService = ownerService;

        [HttpGet]
        public async Task<List<Owner>> Get() =>
            await _ownerService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Owner>> Get(string id)
        {
            var owner = await _ownerService.GetAsync(id);

            if (owner is null)
            {
                return NotFound();
            }

            return owner;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Owner newOwner)
        {
            await _ownerService.CreateAsync(newOwner);

            return CreatedAtAction(nameof(Get), new { id = newOwner.Id }, newOwner);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Owner updatedOwner)
        {
            var owner = await _ownerService.GetAsync(id);

            if (owner is null)
            {
                return NotFound();
            }

            updatedOwner.Id = owner.Id;

            await _ownerService.UpdateAsync(id, updatedOwner);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var owner = await _ownerService.GetAsync(id);

            if (owner is null)
            {
                return NotFound();
            }

            await _ownerService.RemoveAsync(id);

            return NoContent();
        }
    }
}