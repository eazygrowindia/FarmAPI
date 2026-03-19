using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
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

        public ActivityController(ActivityService activityService) =>
            _activityService = activityService;

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

            var newActivity = new Activity
            {
                ActivityId = newActivityDto.ActivityId,
                CropId = newActivityDto.CropId,
                ActivityType = newActivityDto.ActivityType,
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