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
        private readonly DiseaseControlInventoryItemService _diseaseControlInventoryItemService;

        public DiseaseControlInventoryController(DiseaseControlInventoryService diseaseControlInventoryService
            , DiseaseControlInventoryItemService diseaseControlInventoryItemService)
        {
            _diseaseControlInventoryService = diseaseControlInventoryService;
            _diseaseControlInventoryItemService = diseaseControlInventoryItemService;
        }

        [HttpGet("GetAll")]
        public async Task<List<DiseaseControlInventory>> Get() =>
            await _diseaseControlInventoryService.GetAsync();

        [HttpGet("GetAllFarmInventory/{farmId}")]
        public async Task<ActionResult<ApiResponse<DiseaseControlInventoryResponse>>> GetAllFarmInventoryAsync(string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
                return BadRequest();

            ApiResponse<DiseaseControlInventoryResponse> response = new ApiResponse<DiseaseControlInventoryResponse>();
            var farmInventories = await _diseaseControlInventoryService.GetAllFarmInventoryAsync(farmId);

            if (farmInventories == null || farmInventories.Count == 0)
            {
                response.Success = false;
                response.Message = "No data found";
                response.Data = new List<DiseaseControlInventoryResponse>();
                return Ok(response);
            }

            foreach (var inventory in farmInventories)
            {
                var inventoryItems = await _diseaseControlInventoryItemService.GetByInventoryIdAsync(inventory.InventoryId);
                response.Data.Add(new DiseaseControlInventoryResponse
                {
                    FarmId = inventory.FarmId,
                    InventoryId = inventory.InventoryId,
                    SuppliedDate = inventory.SuppliedDate,
                    Supplier = inventory.Supplier,
                    InvoiceNumber = inventory.InvoiceNumber,
                    DiseaseControlItems = inventoryItems
                });
            }
            response.Success = true;
            response.Message = "Get successful";
            return Ok(response);
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

            //start - create disease control inventory document
            var newDiseaseControlInventory = new DiseaseControlInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                FarmId = newDiseaseControlInventoryDto.FarmId,
                SuppliedDate = newDiseaseControlInventoryDto.SuppliedDate.ToUniversalTime(),
                InvoiceNumber = newDiseaseControlInventoryDto.InvoiceNumber,
                Supplier = newDiseaseControlInventoryDto.Supplier,
                CreatedBy = actor,
                UpdatedBy = actor
            };

            var existingDiseaseControlInventory = await _diseaseControlInventoryService.GetByInventoryIdAsync(newDiseaseControlInventory.InventoryId);
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

            await _diseaseControlInventoryService.CreateAsync(newDiseaseControlInventory);
            //end - create disease control inventory document

            //start - create disease control inventory items document
            var newDiseaseControlInventoryItems = newDiseaseControlInventoryDto.DiseaseControlItems.Select(item => new DiseaseControlInventoryItem
            {
                InventoryItemId = Guid.NewGuid().ToString(),
                InventoryId = newDiseaseControlInventory.InventoryId,
                DiseaseControlName = item.DiseaseControlName,
                QuantitySupplied = item.QuantitySupplied,
                QuantityMetric = item.QuantityMetric,
                CreatedBy = actor,
                UpdatedBy = actor
            }).ToList();

            await _diseaseControlInventoryItemService.CreateManyAsync(newDiseaseControlInventoryItems);
            //end - create disease control inventory items document

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

            await _diseaseControlInventoryItemService.RemoveAsync(inventoryId);

            return NoContent();
        }
    }
}