using FarmAPI.Models;
using FarmAPI.Models.Dtos;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<Maintainer>> Get(string id)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);

            if (existingMaintainer is null)
            {
                return NotFound();
            }

            return existingMaintainer;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateMaintainerDto newMaintainerDto)
        {
            var newMaintainer = new Maintainer
            {
                MaintainerId = newMaintainerDto.MaintainerId,
                MaintainerName = newMaintainerDto.MaintainerName,
                ContactNumber = newMaintainerDto.ContactNumber,
                AlternateContactNumber = newMaintainerDto.AlternateContactNumber,
                Address = newMaintainerDto.Address,
                IdentityProofDocument = newMaintainerDto.IdentityProofDocument,
                IdentityProofNumber = newMaintainerDto.IdentityProofNumber,
                FarmsMaintained = newMaintainerDto.FarmsMaintained,
                Role = newMaintainerDto.Role,
                SystemStatus = "Active"
            };

            var existingMaintainer = await _maintainerService.GetAsync(newMaintainer.MaintainerId);
            if(existingMaintainer != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A newMaintainer with MaintainerId '{newMaintainer.MaintainerId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            await _maintainerService.CreateAsync(newMaintainer);

            return CreatedAtAction(nameof(Get), new { id = newMaintainer.Id }, newMaintainer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateMaintainerDto updatedMaintainerDto)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);
            if (existingMaintainer == null) return NotFound();

            existingMaintainer.MaintainerName = updatedMaintainerDto.MaintainerName;
            existingMaintainer.Address = updatedMaintainerDto.Address;
            existingMaintainer.ContactNumber = updatedMaintainerDto.ContactNumber;
            existingMaintainer.AlternateContactNumber = updatedMaintainerDto.AlternateContactNumber;
            existingMaintainer.IdentityProofDocument = updatedMaintainerDto.IdentityProofDocument;
            existingMaintainer.IdentityProofNumber = updatedMaintainerDto.IdentityProofNumber;
            existingMaintainer.FarmsMaintained = updatedMaintainerDto.FarmsMaintained;
            existingMaintainer.Role = updatedMaintainerDto.Role;
            existingMaintainer.SystemStatus = updatedMaintainerDto.SystemStatus;

            await _maintainerService.UpdateAsync(id, existingMaintainer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingMaintainer = await _maintainerService.GetAsync(id);

            if (existingMaintainer is null)
            {
                return NotFound();
            }

            await _maintainerService.RemoveAsync(id);

            return NoContent();
        }
    }
}