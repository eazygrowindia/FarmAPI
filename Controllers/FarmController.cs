using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            if (string.IsNullOrEmpty(searchTerm.Trim()) || searchTerm.Length < 2)
                return BadRequest(new { error = "Search term must be at least 2 characters" });

            //var farms = await _farmService.GetPartialFarmByIdOrName(searchTerm.Trim());

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isFarmOwner = User.IsInRole(UserRoles.FARMOWNER.ToString());
            var isFarmHelp = User.IsInRole(UserRoles.FARMHELP.ToString());

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var farms = await _farmService.GetPartialFarmByIdOrNameOnUser(userId, roles, searchTerm);


            return Ok(farms);
        }

        [HttpGet("GetAllFarmCropByUser")]
        public async Task<ActionResult<IEnumerable<FarmPartial>>> Search()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isFarmOwner = User.IsInRole(UserRoles.FARMOWNER.ToString());
            var isFarmHelp = User.IsInRole(UserRoles.FARMHELP.ToString());

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var farms = await _farmService.GetPartialFarmByUser(userId, roles);

            return Ok(farms);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmDto newFarmDto)
        {
            var newFarm = new Farm
            {
                FarmName = newFarmDto.FarmName,
                FarmId = newFarmDto.FarmId,
                Address = new FarmAddress
                {
                    Pincode = newFarmDto.Address.Pincode,
                    State = newFarmDto.Address.State,
                    District = newFarmDto.Address.District,
                    SubDistrict = newFarmDto.Address.SubDistrict,
                    Village = newFarmDto.Address.Village,
                    AddressLine = newFarmDto.Address.AddressLine,
                    Taluka = newFarmDto.Address.Taluka,
                    Hobli = newFarmDto.Address.Hobli,
                    SurveyNumber = newFarmDto.Address.SurveyNumber,
                    Hissa = newFarmDto.Address.Hissa
                },
                ShadeNetArea = newFarmDto.ShadeNetArea,
                GeoLocation = newFarmDto.GeoLocation,
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
                WeatherData = newFarmDto.historicalWeather,
                FarmOwnerId = newFarmDto.FarmOwnerId,
                FarmMaintainerId = newFarmDto.FarmMaintainerId,
                //Crops = new List<string>()
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
            existingFarm.Address = new FarmAddress
            {
                Pincode = updatedFarmDto.Address.Pincode,
                State = updatedFarmDto.Address.State,
                District = updatedFarmDto.Address.District,
                SubDistrict = updatedFarmDto.Address.SubDistrict,
                Village = updatedFarmDto.Address.Village,
                AddressLine = updatedFarmDto.Address.AddressLine,
                Taluka = updatedFarmDto.Address.Taluka,
                Hobli = updatedFarmDto.Address.Hobli,
                SurveyNumber = updatedFarmDto.Address.SurveyNumber,
                Hissa = updatedFarmDto.Address.Hissa
            };
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

            //if(existingFarm.Crops == null)
            //{
            //    existingFarm.Crops = new List<string>();
            //}
            //existingFarm.Crops = updatedFarmDto.Crops;

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