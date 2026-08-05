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
        private readonly FertilizerInventoryItemService _fertilizerInventoryItemService;

        public FertilizerInventoryController(FertilizerInventoryService fertilizerInventoryService
            , FertilizerInventoryItemService fertilizerInventoryItemService)
        {
            _fertilizerInventoryService = fertilizerInventoryService;
            _fertilizerInventoryItemService = fertilizerInventoryItemService;
        }

        [HttpGet("GetAll")]
        public async Task<List<FertilizerInventory>> Get() =>
            await _fertilizerInventoryService.GetAsync();

        [HttpGet("GetAllFarmInventory/{farmId}")]
        public async Task<ActionResult<ApiResponse<FertilizerInventoryResponse>>> GetAllFarmInventoryAsync(string farmId)
        {
            if (string.IsNullOrEmpty(farmId))
                return BadRequest();

            ApiResponse<FertilizerInventoryResponse> response = new ApiResponse<FertilizerInventoryResponse>();
            var farmInventories = await _fertilizerInventoryService.GetAllFarmInventoryAsync(farmId);

            if (farmInventories == null || farmInventories.Count == 0)
            {
                response.Success = false;
                response.Message = "No data found";
                response.Data = new List<FertilizerInventoryResponse>();
                return Ok(response);
            }

            foreach (var inventory in farmInventories)
            {
                var inventoryItems = await _fertilizerInventoryItemService.GetByInventoryIdAsync(inventory.InventoryId);
                response.Data.Add(new FertilizerInventoryResponse
                {
                    FarmId = inventory.FarmId,
                    InventoryId = inventory.InventoryId,
                    SuppliedDate = inventory.SuppliedDate,
                    Supplier = inventory.Supplier,
                    InvoiceNumber = inventory.InvoiceNumber,
                    FertilizerItems = inventoryItems
                });
            }
            response.Success = true;
            response.Message = "Get successful";
            return Ok(response);
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

            //start - create fertilizer inventory document
            var newFertilizerInventory = new FertilizerInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                FarmId = newFertilizerInventoryDto.FarmId,
                SuppliedDate = newFertilizerInventoryDto.SuppliedDate.ToUniversalTime(),
                InvoiceNumber = newFertilizerInventoryDto.InvoiceNumber,
                Supplier = newFertilizerInventoryDto.Supplier,
                CreatedBy = actor,
                UpdatedBy = actor
            };

            var existingFertilizerInventory = await _fertilizerInventoryService.GetByInventoryIdAsync(newFertilizerInventory.InventoryId);
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

            await _fertilizerInventoryService.CreateAsync(newFertilizerInventory);
            //end - create fertilizer inventory document

            //start - create fertilizer inventory items document
            var newFertilizerInventoryItems = newFertilizerInventoryDto.FertilizerItems.Select(item => new FertilizerInventoryItem
            {
                InventoryItemId = Guid.NewGuid().ToString(),
                InventoryId = newFertilizerInventory.InventoryId,
                FertilizerName = item.FertilizerName,
                QuantitySupplied = item.QuantitySupplied,
                QuantityMetric = item.QuantityMetric,
                CreatedBy = actor,
                UpdatedBy = actor
            }).ToList();

            await _fertilizerInventoryItemService.CreateManyAsync(newFertilizerInventoryItems);
            //foreach (var item in newFertilizerInventoryItems)
            //{
            //    await _fertilizerInventoryItemService.CreateAsync(item);
            //}

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

            await _fertilizerInventoryItemService.RemoveAsync(inventoryId);

            return NoContent();
        }

        [HttpGet("GetInputCatalogNames/{type}")]
        public async Task<ActionResult<List<string>>> GetInputCatalogNames(string type)
        {
            if (string.IsNullOrEmpty(type))
                return BadRequest("Type parameter is required.");

            var names = await _fertilizerInventoryService.GetInputCatalogNamesAsync(type);
            return Ok(names);
        }

        [HttpPost("CreateInputCatalog")]
        public async Task<IActionResult> CreateInputCatalog([FromBody] CreateInputCatalogDto newCatalogDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _fertilizerInventoryService.GetInputCatalogByNameAndTypeAsync(newCatalogDto.Name, newCatalogDto.Type);
            if (existing != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"An input catalog item with name '{newCatalogDto.Name}' and type '{newCatalogDto.Type}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            var newCatalog = new InputCatalog
            {
                Type = newCatalogDto.Type,
                Name = newCatalogDto.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _fertilizerInventoryService.CreateInputCatalogAsync(newCatalog);
            return Ok(newCatalog);
        }

        [HttpDelete("RemoveInputCatalog/{type}/{name}")]
        public async Task<IActionResult> RemoveInputCatalog(string type, string name)
        {
            Console.WriteLine($"[DEBUG] RemoveInputCatalog called with type: '{type}', name: '{name}'");
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
                return BadRequest("Type and name parameters are required.");

            var existing = await _fertilizerInventoryService.GetInputCatalogByNameAndTypeAsync(name, type);
            Console.WriteLine($"[DEBUG] Existing catalog item found: {existing != null}");
            if (existing == null)
                return NotFound();

            await _fertilizerInventoryService.RemoveInputCatalogAsync(name, type);
            return NoContent();
        }

        [HttpPut("UpdateInputCatalog")]
        public async Task<IActionResult> UpdateInputCatalog([FromBody] UpdateInputCatalogDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _fertilizerInventoryService.GetInputCatalogByNameAndTypeAsync(updateDto.OldName, updateDto.Type);
            if (existing == null)
                return NotFound("Original item not found.");

            if (updateDto.OldName.ToLower() != updateDto.NewName.ToLower())
            {
                var newExisting = await _fertilizerInventoryService.GetInputCatalogByNameAndTypeAsync(updateDto.NewName, updateDto.Type);
                if (newExisting != null)
                {
                    var problem = new ProblemDetails
                    {
                        Title = "Item already exists",
                        Detail = $"An input catalog item with name '{updateDto.NewName}' and type '{updateDto.Type}' already exists.",
                        Status = StatusCodes.Status400BadRequest,
                        Instance = HttpContext.Request.Path                 
                    };
                    return BadRequest(problem);
                }
            }

            await _fertilizerInventoryService.UpdateInputCatalogAsync(updateDto.OldName, updateDto.NewName, updateDto.Type);
            return Ok();
        }
    }
}