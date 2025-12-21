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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FarmPartial>>> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return BadRequest(new { error = "Search term must be at least 2 characters" });

            var farms = await _farmService.GetPartialFarmByIdOrName(searchTerm.Trim());

            return Ok(farms);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmDto newFarmDto)
        {
            var newFarm = new Farm
            {
                FarmName = newFarmDto.FarmName,
                FarmId = newFarmDto.FarmId,
                SurveyNumber = newFarmDto.SurveyNumber,
                Address = newFarmDto.Address,
                ShadeNetArea = newFarmDto.ShadeNetArea,
                //GeoTag = newFarmDto.GeoTag,
                FarmPondVolume = newFarmDto.FarmPondVolume,
                IsSolarPowerAvailable = newFarmDto.IsSolarPowerAvailable,
                MotorCapacity = newFarmDto.MotorCapacity,
                AdditionalWaterSource = newFarmDto.AdditionalWaterSource,
                WaterTestCertificateUrl = newFarmDto.WaterTestCertificateUrl,
                IsSinglePhasePower = newFarmDto.IsSinglePhasePower,
                IsThreePhasePower = newFarmDto.IsThreePhasePower,
                //GridPowerUnAvailability = newFarmDto.GridPowerUnAvailability,
                AutomationRoomSize = newFarmDto.AutomationRoomSize,
                //FarmhouseNote = newFarmDto.FarmhouseNote,
                StorageAreaNote = newFarmDto.StorageAreaNote,
                Crops = new List<string>()
            };

            //REVIEW: Check based on FarmId uniqueness is not enough, as it is by default unique.
            //Check has to be done on FarmName as well to avoid duplicate farms.
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
            existingFarm.SurveyNumber = updatedFarmDto.SurveyNumber;
            existingFarm.Address = updatedFarmDto.Address;
            existingFarm.ShadeNetArea = updatedFarmDto.ShadeNetArea;
            //existingFarm.GeoTag = updatedFarmDto.GeoTag;
            existingFarm.FarmPondVolume = updatedFarmDto.FarmPondVolume;
            existingFarm.IsSolarPowerAvailable = updatedFarmDto.IsSolarPowerAvailable;
            existingFarm.MotorCapacity = updatedFarmDto.MotorCapacity;
            existingFarm.AdditionalWaterSource = updatedFarmDto.AdditionalWaterSource;
            existingFarm.WaterTestCertificateUrl = updatedFarmDto.WaterTestCertificateUrl;
            existingFarm.IsSinglePhasePower = updatedFarmDto.IsSinglePhasePower;
            existingFarm.IsThreePhasePower = updatedFarmDto.IsThreePhasePower;
            //existingFarm.GridPowerUnAvailability = updatedFarmDto.GridPowerUnAvailability;
            existingFarm.AutomationRoomSize = updatedFarmDto.AutomationRoomSize;
            //existingFarm.FarmhouseNote = updatedFarmDto.FarmhouseNote;
            existingFarm.StorageAreaNote = updatedFarmDto.StorageAreaNote;

            if(existingFarm.Crops == null)
            {
                existingFarm.Crops = new List<string>();
            }
            existingFarm.Crops = updatedFarmDto.Crops;

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