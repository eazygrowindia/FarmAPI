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
    public class ActivityController : ControllerBase
    {
        private readonly ActivityService _activityService;
        private readonly CropService _cropService;
        private readonly FertilizerInventoryService _fertilizerInventoryService;
        private readonly FertilizerInventoryItemService _fertilizerInventoryItemService;
        private readonly DiseaseControlInventoryService _diseaseControlInventoryService;
        private readonly DiseaseControlInventoryItemService _diseaseControlInventoryItemService;

        public ActivityController(ActivityService activityService, CropService cropService
            , FertilizerInventoryService fertilizerInventoryService, FertilizerInventoryItemService fertilizerInventoryItemService
            , DiseaseControlInventoryService diseaseControlInventoryService, DiseaseControlInventoryItemService diseaseControlInventoryItemService)
        {
            _activityService = activityService;
            _cropService = cropService;
            _fertilizerInventoryService = fertilizerInventoryService;
            _fertilizerInventoryItemService = fertilizerInventoryItemService;
            _diseaseControlInventoryItemService = diseaseControlInventoryItemService;
             _diseaseControlInventoryService = diseaseControlInventoryService;
        }

        [HttpGet]
        public async Task<List<Activity>> Get() =>
            await _activityService.GetAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> Get(string id)
        {
            var activity = await _activityService.GetByActivityIdAsync(id);

            if (activity is null)
            {
                return NotFound();
            }

            return activity;
        }

        [HttpGet("CropId/{id}")]
        public async Task<ActionResult<List<Activity>>> GetByCropId(string id)
        {
            var activities = await _activityService.GetByCropIdAsync(id);

            if (activities is null || activities.Count <= 0)
            {
                return NotFound();
            }

            return activities;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateActivityDto newActivityDto)
        {
            // dto.ActivityName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            var actor = User.GetActor();
            if (string.IsNullOrEmpty(actor))
                return UnprocessableEntity();

            var newActivity = new Activity
            {
                ActivityId = newActivityDto.ActivityId,
                CropId = newActivityDto.CropId,
                ActivityType = newActivityDto.ActivityType,
                ProductName = newActivityDto.ProductName,
                Quantity = newActivityDto.Quantity,
                Message = newActivityDto.Message,
                ImageUrl = newActivityDto.Photo
            };

            var existingActivity = await _activityService.GetByActivityIdAsync(newActivity.ActivityId);
            if (existingActivity != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"An activity with ActivityId '{newActivity.ActivityId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            await _activityService.CreateAsync(newActivity);

            // if ProductName is provided, try to find it in inventory and update the quantity used accordingly
            if (!string.IsNullOrEmpty(newActivity.ProductName))
            {
                var farmId = _cropService.GetByCropIdAsync(newActivity.CropId).Result.FarmId;
                if (!string.IsNullOrEmpty(farmId)) 
                {
                    // Check fertilizer inventory first
                    var fertilizerInventories = await _fertilizerInventoryService.GetAllFarmInventoryAsync(farmId);
                    var fertilizerInventoryIds = fertilizerInventories.Select(f => f.InventoryId).ToList();
                    var fertilizerItems = await _fertilizerInventoryItemService.GetByMultipleInventoryIdFertilizerNameAsync(fertilizerInventoryIds, newActivity.ProductName);

                    // Check disease control inventory
                    var diseaseControlInventories = await _diseaseControlInventoryService.GetAllFarmInventoryAsync(farmId);
                    var diseaseControlInventoryIds = diseaseControlInventories.Select(f => f.InventoryId).ToList();
                    var diseaseControlItems = await _diseaseControlInventoryItemService.GetByMultipleInventoryIdDiseaseControlNameAsync(diseaseControlInventoryIds, newActivity.ProductName);

                    // Combine both lists and sort by ascending/earliest first
                    var allProductItems = new List<dynamic>();

                    var fertilizerItemsForProcessing = from items in fertilizerItems
                                join inventories in fertilizerInventories on items.InventoryId equals inventories.InventoryId into inventoryItems
                                from inventoryItem in inventoryItems.DefaultIfEmpty()
                                select new
                                {
                                    Item = (object)items,
                                    Type = "fertilizer",
                                    SuppliedDate = inventoryItem?.SuppliedDate
                                };

                    var diseaseControlItemsForProcessing = from items in diseaseControlItems
                                join inventories in diseaseControlInventories on items.InventoryId equals inventories.InventoryId into inventoryItems
                                from inventoryItem in inventoryItems.DefaultIfEmpty()
                                select new
                                {
                                    Item = (object)items,
                                    Type = "diseaseControl",
                                    SuppliedDate = inventoryItem?.SuppliedDate
                                };


                    allProductItems.AddRange(fertilizerItemsForProcessing);
                    allProductItems.AddRange(diseaseControlItemsForProcessing);

                    var orderedProductItems = allProductItems
                        .OrderBy(x => x.SuppliedDate)
                        .ToList();

                    foreach (var productWrapper in orderedProductItems)
                    {
                        if (productWrapper.Type == "fertilizer")
                        {
                            var fertilizerItem = (FertilizerInventoryItem)productWrapper.Item;
                            fertilizerItem.UpdatedBy = actor;
                            double availableQuantity = fertilizerItem.QuantitySupplied - fertilizerItem.QuantityUsed;

                            if (availableQuantity > 0)
                            {
                                if (availableQuantity >= newActivity.Quantity)
                                {
                                    fertilizerItem.QuantityUsed += newActivity.Quantity.Value;
                                    await _fertilizerInventoryItemService.ConsumeQuantityAsync(fertilizerItem.InventoryItemId, fertilizerItem);
                                    break; // done updating, exit loop
                                }
                                else
                                {
                                    // use up the remaining available quantity and continue to next item
                                    fertilizerItem.QuantityUsed += availableQuantity;
                                    newActivity.Quantity -= availableQuantity;
                                    await _fertilizerInventoryItemService.ConsumeQuantityAsync(fertilizerItem.InventoryItemId, fertilizerItem);
                                }
                            }
                        }
                        else if (productWrapper.Type == "diseaseControl")
                        {
                            var diseaseControlItem = (DiseaseControlInventoryItem)productWrapper.Item;
                            diseaseControlItem.UpdatedBy = actor;
                            double availableQuantity = diseaseControlItem.QuantitySupplied - diseaseControlItem.QuantityUsed;

                            if (availableQuantity > 0)
                            {
                                if (availableQuantity >= newActivity.Quantity)
                                {
                                    diseaseControlItem.QuantityUsed += newActivity.Quantity.Value;
                                    await _diseaseControlInventoryItemService.ConsumeQuantityAsync(diseaseControlItem.InventoryItemId, diseaseControlItem);
                                    break; // done updating, exit loop
                                }
                                else
                                {
                                    // use up the remaining available quantity and continue to next item
                                    diseaseControlItem.QuantityUsed += availableQuantity;
                                    newActivity.Quantity -= availableQuantity;
                                    await _diseaseControlInventoryItemService.ConsumeQuantityAsync(diseaseControlItem.InventoryItemId, diseaseControlItem);
                                }
                            }
                        }
                    }
                }
            }

            return CreatedAtAction(nameof(Get), new { id = newActivity.Id }, newActivity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateActivityDto updatedActivityDto)
        {
            // dto.ActivityName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            //TODO - take a look at it when update is needed
            var existingActivity = await _activityService.GetByActivityIdAsync(id);
            if (existingActivity == null || existingActivity.CropId != updatedActivityDto.CropId) return NotFound();

            existingActivity.ActivityType = updatedActivityDto.ActivityType;
            existingActivity.Message = updatedActivityDto.Message;

            await _activityService.UpdateAsync(id, existingActivity);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingActivity = await _activityService.GetByActivityIdAsync(id);

            if (existingActivity is null)
            {
                return NotFound();
            }

            await _activityService.RemoveAsync(id);

            return NoContent();
        }
    }
}