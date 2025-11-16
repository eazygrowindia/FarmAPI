using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmController : ControllerBase
    {
        private readonly FarmService _farmService;

        public FarmController(FarmService farmService) =>
            _farmService = farmService;

        [HttpGet]
        public async Task<List<Farm>> Get() =>
            await _farmService.GetAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Farm>> Get(string id)
        {
            var farm = await _farmService.GetAsync(id);

            if (farm is null)
            {
                return NotFound();
            }

            return farm;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmDto dto)
        {
            var farm = new Farm
            {
                FarmName = dto.FarmName,
                FarmId = dto.FarmId,
                Address = dto.Address,
                GPSLocation = dto.GPSLocation,
                SizeInSqMtrs = dto.SizeInSqMtrs
            };

            var existing = await _farmService.GetAsync(farm.FarmId);
            if (existing != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A farm with FarmId '{farm.FarmId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            await _farmService.CreateAsync(farm);
            return CreatedAtAction(nameof(Get), new { id = farm.Id }, farm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateFarmDto dto)
        {
            // Use route id; ignore any Id/FarmId in body
            var existing = await _farmService.GetAsync(id);
            if (existing == null) return NotFound();

            existing.FarmName = dto.FarmName;
            existing.Address = dto.Address;
            existing.GPSLocation = dto.GPSLocation;
            existing.SizeInSqMtrs = dto.SizeInSqMtrs;

            await _farmService.UpdateAsync(id, existing);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var farm = await _farmService.GetAsync(id);

            if (farm is null)
            {
                return NotFound();
            }

            await _farmService.RemoveAsync(id);

            return NoContent();
        }
    }
}