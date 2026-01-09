using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using FarmAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FertilizerInventoryController : ControllerBase
    {
        private readonly FertilizerInventoryService _fertilizerInventoryService;

        public FertilizerInventoryController(FertilizerInventoryService fertilizerInventoryService) =>
            _fertilizerInventoryService = fertilizerInventoryService;

        [HttpGet("GetAll")]
        public async Task<List<FertilizerInventory>> Get() =>
            await _fertilizerInventoryService.GetAsync();

        [HttpGet("GetAllFarmInventory/{farmId}")]
        public async Task<ActionResult<List<FertilizerInventory>>> GetAllFarmInventoryAsync(string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
                return BadRequest();

            return await _fertilizerInventoryService.GetAllFarmInventoryAsync(farmId);
        }

        [HttpGet("GetFertilizerInventoryById/{id}")]
        public async Task<ActionResult<FertilizerInventory>> Get(string id)
        {
            var fertilizerInventory = await _fertilizerInventoryService.GetByIdAsync(id);

            if (fertilizerInventory is null)
            {
                return NotFound();
            }

            return fertilizerInventory;
        }

        [HttpGet("GetFertilizerInventoryByInventoryId/{inventoryId}")]
        public async Task<ActionResult<FertilizerInventory>> GetByInventoryId(string inventoryId)
        {
            var fertilizerInventory = await _fertilizerInventoryService.GetByInventoryIdAsync(inventoryId);

            if (fertilizerInventory is null)
            {
                return NotFound();
            }

            return fertilizerInventory;
        }

        [HttpPost("AddFertilizerInventory")]
        public async Task<IActionResult> Create([FromBody] CreateFertilizerInventoryDto newFertilizerInventoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actor = User.GetActor();
            if(string.IsNullOrEmpty(actor))
                return UnprocessableEntity();

            var newFertilizerInventory = new FertilizerInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                FertilizerName = newFertilizerInventoryDto.FertilizerName,
                FarmId = newFertilizerInventoryDto.FarmId,
                QuantitySupplied = newFertilizerInventoryDto.QuantitySupplied,
                SuppliedDate = newFertilizerInventoryDto.SuppliedDate.ToUniversalTime(),
                CreatedBy = actor,
                UpdatedBy = actor
            };

            var existingFertilizerInventory = await _fertilizerInventoryService.GetByInventoryIdAsync(newFertilizerInventory.InventoryId);
            var existingFertilizerInventoryForFarm = await _fertilizerInventoryService.GetByFarmIdFertilizerNameAsync(newFertilizerInventory.FarmId, newFertilizerInventory.FertilizerName);
            if (existingFertilizerInventory != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A fertilizerInventory with the same inventoryId already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            if (existingFertilizerInventoryForFarm != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A fertilizerInventory for the farm with the same fertillizer already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            await _fertilizerInventoryService.CreateAsync(newFertilizerInventory);
            return CreatedAtAction(nameof(Get), new { id = newFertilizerInventory.Id }, newFertilizerInventory);
        }

        [HttpDelete("RemoveInventory/{inventoryId}")]
        public async Task<IActionResult> Delete(string inventoryId)
        {
            var existingFertilizerInventory = await _fertilizerInventoryService.GetByInventoryIdAsync(inventoryId);

            if (existingFertilizerInventory is null)
            {
                return NotFound();
            }

            await _fertilizerInventoryService.RemoveAsync(inventoryId);

            return NoContent();
        }
    }
}