using FarmAPI.Models;
using FarmAPI.Models.Dtos;
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
            var existingOwner = await _ownerService.GetAsync(id);

            if (existingOwner is null)
            {
                return NotFound();
            }

            return existingOwner;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOwnerDto newOwnerDto)
        {
            var newOwner = new Owner
            {
                OwnerId = newOwnerDto.OwnerId,
                OwnerName = newOwnerDto.OwnerName,
                IdentityProofDocument = newOwnerDto.IdentityProofDocument,
                IdentityProofNumber = newOwnerDto.IdentityProofNumber,
                ContactNumber = newOwnerDto.ContactNumber,
                AlternateContactNumber = newOwnerDto.AlternateContactNumber,
                Address = newOwnerDto.Address,
                EmailId = newOwnerDto.EmailId,
                FarmsOwned = newOwnerDto.FarmsOwned,
                Maintainers = newOwnerDto.Maintainers,
                SystemStatus = "Active"
            };

            //TODO: Check based on OwnerId uniqueness is not enough, as it is by default unique.
            //Check has to be done on OwnerName/FarmerName as well to avoid duplicate farms.
            var existingOwner = await _ownerService.GetAsync(newOwner.OwnerId);
            if (existingOwner != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A maintainer with MaintainerId '{newOwner.OwnerId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            await _ownerService.CreateAsync(newOwner);

            return CreatedAtAction(nameof(Get), new { id = newOwner.Id }, newOwner);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateOwnerDto updatedOwnerDto)
        {
            var existingOwner = await _ownerService.GetAsync(id);

            if (existingOwner is null)
            {
                return NotFound();
            }

            // Map allowed updatable fields from DTO onto the existing entity
            existingOwner.OwnerName = updatedOwnerDto.OwnerName;
            existingOwner.IdentityProofDocument = updatedOwnerDto.IdentityProofDocument;
            existingOwner.IdentityProofNumber = updatedOwnerDto.IdentityProofNumber;
            existingOwner.ContactNumber = updatedOwnerDto.ContactNumber;
            existingOwner.AlternateContactNumber = updatedOwnerDto.AlternateContactNumber;
            existingOwner.Address = updatedOwnerDto.Address;
            existingOwner.EmailId = updatedOwnerDto.EmailId;
            existingOwner.FarmsOwned = updatedOwnerDto.FarmsOwned;
            existingOwner.Maintainers = updatedOwnerDto.Maintainers;
            existingOwner.SystemStatus = updatedOwnerDto.SystemStatus;

            await _ownerService.UpdateAsync(id, existingOwner);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingOwner = await _ownerService.GetAsync(id);

            if (existingOwner is null)
            {
                return NotFound();
            }

            await _ownerService.RemoveAsync(id);

            return NoContent();
        }
    }
}