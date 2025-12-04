using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CropController : ControllerBase
    {
        private readonly CropService _cropService;

        public CropController(CropService cropService) =>
            _cropService = cropService;

        [HttpGet]
        public async Task<List<Crop>> Get() =>
            await _cropService.GetAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Crop>> Get(string id)
        {
            var existingCrop = await _cropService.GetAsync(id);

            if (existingCrop is null)
            {
                return NotFound();
            }

            return existingCrop;
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
                CropArea = newCropDto.CropArea,
                DateOfSowing = newCropDto.DateOfSowing
            };

            //TODO: Check based on CropId uniqueness is not enough, as it is by default unique.
            //Check has to be done on CropName as well to avoid duplicate crops.
            var existingCrop = await _cropService.GetAsync(newCrop.CropId);
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

            //TODO : Business logic to calculate ProbableHarvestDate based on Crop type can be added here.
            newCrop.ProbableHarvestDate = newCropDto.DateOfSowing.AddMonths(6);
            newCrop.ExpectedYield = 25;

            await _cropService.CreateAsync(newCrop);
            return CreatedAtAction(nameof(Get), new { id = newCrop.Id }, newCrop);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCropDto updatedCropDto)
        {
            // dto.CropName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            // Use route id; ignore any Id/CropId in body
            var existingCrop = await _cropService.GetAsync(id);
            if (existingCrop == null) return NotFound();

            existingCrop.CropName = updatedCropDto.CropName;
            existingCrop.CropArea = updatedCropDto.CropArea;
            existingCrop.DateOfSowing = updatedCropDto.DateOfSowing;

            //TODO : Business logic to calculate ProbableHarvestDate based on Crop type can be added here.
            if(updatedCropDto.DateOfSowing != existingCrop.DateOfSowing)
                existingCrop.ProbableHarvestDate = updatedCropDto.DateOfSowing.AddMonths(6);

            existingCrop.ExpectedYield = 25;

            await _cropService.UpdateAsync(id, existingCrop);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingCrop = await _cropService.GetAsync(id);

            if (existingCrop is null)
            {
                return NotFound();
            }

            await _cropService.RemoveAsync(id);

            return NoContent();
        }
    }
}