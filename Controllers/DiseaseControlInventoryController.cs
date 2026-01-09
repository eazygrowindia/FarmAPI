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
    public class DiseaseControlInventoryController : ControllerBase
    {
        private readonly DiseaseControlInventoryService _diseaseControlInventoryService;

        public DiseaseControlInventoryController(DiseaseControlInventoryService diseaseControlInventoryService) =>
            _diseaseControlInventoryService = diseaseControlInventoryService;

        [HttpGet("GetAll")]
        public async Task<List<DiseaseControlInventory>> Get() =>
            await _diseaseControlInventoryService.GetAsync();

        [HttpGet("GetAllFarmInventory/{farmId}")]
        public async Task<ActionResult<List<DiseaseControlInventory>>> GetAllFarmInventoryAsync(string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
                return BadRequest();

            return await _diseaseControlInventoryService.GetAllFarmInventoryAsync(farmId);
        }

        [HttpGet("GetDiseaseControlInventoryById/{id}")]
        public async Task<ActionResult<DiseaseControlInventory>> Get(string id)
        {
            var diseaseControlInventory = await _diseaseControlInventoryService.GetByIdAsync(id);

            if (diseaseControlInventory is null)
            {
                return NotFound();
            }

            return diseaseControlInventory;
        }

        [HttpGet("GetDiseaseControlInventoryByInventoryId/{inventoryId}")]
        public async Task<ActionResult<DiseaseControlInventory>> GetByInventoryId(string inventoryId)
        {
            var diseaseControlInventory = await _diseaseControlInventoryService.GetByInventoryIdAsync(inventoryId);

            if (diseaseControlInventory is null)
            {
                return NotFound();
            }

            return diseaseControlInventory;
        }

        [HttpPost("AddDiseaseControlInventory")]
        public async Task<IActionResult> Create([FromBody] CreateDiseaseControlInventoryDto newDiseaseControlInventoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actor = User.GetActor();
            if(string.IsNullOrEmpty(actor))
                return UnprocessableEntity();

            var newDiseaseControlInventory = new DiseaseControlInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                DiseaseControlName = newDiseaseControlInventoryDto.DiseaseControlName,
                FarmId = newDiseaseControlInventoryDto.FarmId,
                QuantitySupplied = newDiseaseControlInventoryDto.QuantitySupplied,
                SuppliedDate = newDiseaseControlInventoryDto.SuppliedDate.ToUniversalTime(),
                CreatedBy = actor,
                UpdatedBy = actor
            };

            var existingDiseaseControlInventory = await _diseaseControlInventoryService.GetByInventoryIdAsync(newDiseaseControlInventory.InventoryId);
            var existingFertilizerInventoryForFarm = await _diseaseControlInventoryService.GetByFarmIdDiseaseControlNameAsync(newDiseaseControlInventory.FarmId, newDiseaseControlInventory.DiseaseControlName);
            if (existingDiseaseControlInventory != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A diseaseControlInventory with the same inventoryId already exists.",
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
                    Detail = $"A diseaseControlInventory for the farm with the same diseaseControl already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            await _diseaseControlInventoryService.CreateAsync(newDiseaseControlInventory);
            return CreatedAtAction(nameof(Get), new { id = newDiseaseControlInventory.Id }, newDiseaseControlInventory);
        }

        [HttpDelete("RemoveInventory/{inventoryId}")]
        public async Task<IActionResult> Delete(string inventoryId)
        {
            var existingDiseaseControlInventory = await _diseaseControlInventoryService.GetByInventoryIdAsync(inventoryId);

            if (existingDiseaseControlInventory is null)
            {
                return NotFound();
            }

            await _diseaseControlInventoryService.RemoveAsync(inventoryId);

            return NoContent();
        }
    }
}