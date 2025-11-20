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
            var existingFarm = await _farmService.GetAsync(id);

            if (existingFarm is null)
            {
                return NotFound();
            }

            return existingFarm;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmDto newFarmDto)
        {
            var newFarm = new Farm
            {
                FarmName = newFarmDto.FarmName,
                FarmId = newFarmDto.FarmId,
                Address = newFarmDto.Address,
                GPSLocation = newFarmDto.GPSLocation,
                ShadeNetArea = newFarmDto.ShadeNetArea
            };

            var existingFarm = await _farmService.GetAsync(newFarm.FarmId);
            if (existingFarm != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A farm with FarmId '{newFarm.FarmId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            await _farmService.CreateAsync(newFarm);
            return CreatedAtAction(nameof(Get), new { id = newFarm.Id }, newFarm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateFarmDto updatedFarmDto)
        {
            // Use route id; ignore any Id/FarmId in body
            var existingFarm = await _farmService.GetAsync(id);
            if (existingFarm == null) return NotFound();

            existingFarm.FarmName = updatedFarmDto.FarmName;
            existingFarm.Address = updatedFarmDto.Address;
            existingFarm.GPSLocation = updatedFarmDto.GPSLocation;
            existingFarm.ShadeNetArea = updatedFarmDto.ShadeNetArea;

            await _farmService.UpdateAsync(id, existingFarm);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingFarm = await _farmService.GetAsync(id);

            if (existingFarm is null)
            {
                return NotFound();
            }

            await _farmService.RemoveAsync(id);

            return NoContent();
        }
    }
}