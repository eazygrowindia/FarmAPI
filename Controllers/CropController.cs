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
    public class CropController : ControllerBase
    {
        private readonly CropService _cropService;
        private readonly CropMasterService _cropMasterService;

        public CropController(CropService cropService, CropMasterService cropMasterService)
        {
            _cropService = cropService;
            _cropMasterService = cropMasterService;
        }

        [HttpGet]
        public async Task<List<Crop>> Get() =>
            await _cropService.GetAsync();

        [HttpGet("{cropId}")]
        public async Task<ActionResult<Crop>> Get(string cropId)
        {
            var existingCrop = await _cropService.GetByCropIdAsync(cropId);

            if (existingCrop is null)
            {
                return NotFound();
            }

            return existingCrop;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CropPartial>>> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                return BadRequest(new { error = "Search term must be at least 2 characters" });

            var farms = await _cropService.GetPartialCropByIdOrName(searchTerm.Trim());

            return Ok(farms);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCropDto newCropDto)
        {
            // dto.CropName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            var newCrop = new Crop
            {
                CropName = newCropDto.CropName,
                CropId = newCropDto.CropId,
                FarmId = newCropDto.FarmId,
                CropMasterId = newCropDto.CropMasterId,
                CropArea = newCropDto.CropArea,
                DateOfSowing = newCropDto.DateOfSowing
            };

            //Check has to be done on CropName as well to avoid duplicate crops.
            var existingCrop = await _cropService.GetByCropIdAsync(newCrop.CropId);
            if (existingCrop != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A crop with CropId '{newCrop.CropId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            //Check based on farmId has to be included so that no two crops of a farm can have same master crop Ids
            var existingCropWithCropMaster = await _cropService.GetByFarmAndCropMasterAsync(newCrop.FarmId, newCrop.CropMasterId);
            if (existingCrop != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A crop with CropId '{newCrop.CropId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            //Business logic to calculate ProbableHarvestDate based on Crop type
            var cropMasterData = await _cropMasterService.GetByCropIdAsync(newCrop.CropMasterId);
            if (cropMasterData != null)
            {
                newCrop.ProbableHarvestDate = newCropDto.DateOfSowing.AddDays(cropMasterData.Duration);
                newCrop.ExpectedYield = newCropDto.CropArea * cropMasterData.ExpectedYield;
            }
            else
            {
                // Default logic if CropMaster data is not found
                newCrop.ProbableHarvestDate = DateTime.MinValue;
                newCrop.ExpectedYield = 0;
            }

            await _cropService.CreateAsync(newCrop);
            return CreatedAtAction(nameof(Get), new { id = newCrop.Id }, newCrop);
        }

        [HttpPut("{cropId}")]
        public async Task<IActionResult> Update(string cropId, [FromBody] UpdateCropDto updatedCropDto)
        {
            // dto.CropName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            // Use route id; ignore any Id/CropId in body
            var existingCrop = await _cropService.GetByCropIdAsync(cropId);
            if (existingCrop == null) return NotFound();

            existingCrop.CropName = updatedCropDto.CropName;
            existingCrop.CropArea = updatedCropDto.CropArea;
            existingCrop.DateOfSowing = updatedCropDto.DateOfSowing;

            //TODO : Business logic to calculate ProbableHarvestDate based on Crop type can be added here.
            if(updatedCropDto.DateOfSowing != existingCrop.DateOfSowing)
                existingCrop.ProbableHarvestDate = updatedCropDto.DateOfSowing.AddMonths(6);

            existingCrop.ExpectedYield = 25;

            await _cropService.UpdateAsync(cropId, existingCrop);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{cropId}")]
        public async Task<IActionResult> Delete(string cropId)
        {
            var existingCrop = await _cropService.GetByCropIdAsync(cropId);

            if (existingCrop is null)
            {
                return NotFound();
            }

            await _cropService.RemoveAsync(cropId);

            return NoContent();
        }
    }
}