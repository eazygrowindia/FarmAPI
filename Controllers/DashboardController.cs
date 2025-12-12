using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly ActivityService _activityService;
        private readonly FarmService _farmService;
        private readonly CropService _cropService;
        public DashboardController(ActivityService activityService, FarmService farmService, CropService cropService)
        {
            _activityService = activityService;
            _farmService = farmService;
            _cropService = cropService;
        }

        [HttpGet("Summary")]
        public async Task<ActionResult<Summary>> Summary()
        {
            var numberOfDistinctCrops = await _cropService.GetDistinctCropsCountAsync();
            var numberOfDistinctFarms = await _farmService.GetDistinctFarmsCountAsync();

            Summary summary = new Summary
            {
                TotalFarms = (int)numberOfDistinctFarms,
                TotalCrops = (int)numberOfDistinctCrops
            };

            return summary;
        }

        [HttpGet("Activity/Today/Count")]
        public async Task<long> ActivitiesTodayCount()
        {
            var newActivitiesToday = await _activityService.GetTodayNewActivityCountAsync();

            return newActivitiesToday;
        }
    }
}
