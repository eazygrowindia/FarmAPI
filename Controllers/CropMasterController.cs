using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Services;
using FarmAPI.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CropMasterController : ControllerBase
    {
        private readonly CropMasterService _cropMasterService;

        public CropMasterController(CropMasterService observationService) =>
            _cropMasterService = observationService;

        [HttpGet("GetAll")]
        public async Task<List<CropMaster>> Get() =>
            await _cropMasterService.GetAsync();

        [HttpGet("GetCropMasterById/{id}")]
        public async Task<ActionResult<CropMaster>> Get(string id)
        {
            var cropMaster = await _cropMasterService.GetByIdAsync(id);

            if (cropMaster is null)
            {
                return NotFound();
            }

            return cropMaster;
        }

        [HttpGet("GetCropMasterByCropId/{cropId}")]
        public async Task<ActionResult<CropMaster>> GetByCropId(string cropId)
        {
            var cropMaster = await _cropMasterService.GetByCropIdAsync(cropId);

            if (cropMaster is null)
            {
                return NotFound();
            }

            return cropMaster;
        }

        [HttpPost("AddCropMaster")]
        public async Task<IActionResult> Create([FromBody] CreateCropMasterDto newCropMasterDto)
        {
            // dto.ObservationName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            var actor = User.GetActor();
            if(string.IsNullOrEmpty(actor))
                return UnprocessableEntity();

            var newCropMaster = new CropMaster
            {
                CropId = Guid.NewGuid().ToString(),
                CropName = newCropMasterDto.CropName,
                Duration = newCropMasterDto.Duration,
                ExpectedYield = newCropMasterDto.ExpectedYield,
                SowingTime = newCropMasterDto.SowingTime,
                HarvestTime = newCropMasterDto.HarvestTime,
                SowingMethod = newCropMasterDto.SowingMethod,
                PestsAndDiseases = newCropMasterDto.PestsAndDiseases,
                MoleculesToAdd = newCropMasterDto.MoleculesToAdd,
                CreatedBy = actor,
                UpdatedBy = actor
            };

            var existingCropMaster = await _cropMasterService.GetByCropIdAsync(newCropMaster.CropId);
            if (existingCropMaster != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"An cropMaster with CropId '{newCropMaster.CropId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            await _cropMasterService.CreateAsync(newCropMaster);
            return CreatedAtAction(nameof(Get), new { id = newCropMaster.Id }, newCropMaster);
        }

        [HttpPut("UpdateCropMaster/{cropId}")]
        public async Task<IActionResult> Update(string cropId, [FromBody] UpdateCropMasterDto updatedCropMasterDto)
        {
            // dto.ObservationName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            var actor = User.GetActor();
            if (string.IsNullOrEmpty(actor))
                return UnprocessableEntity();

            //TODO - take a look at it when update is needed
            var existingCropMaster = await _cropMasterService.GetByCropIdAsync(cropId);
            if (existingCropMaster == null) return NotFound();

            existingCropMaster.CropName = updatedCropMasterDto.CropName;
            existingCropMaster.Duration = updatedCropMasterDto.Duration;
            existingCropMaster.ExpectedYield = updatedCropMasterDto.ExpectedYield;
            existingCropMaster.SowingTime = updatedCropMasterDto.SowingTime;
            existingCropMaster.HarvestTime = updatedCropMasterDto.HarvestTime;
            existingCropMaster.SowingMethod = updatedCropMasterDto.SowingMethod;
            existingCropMaster.PestsAndDiseases = updatedCropMasterDto.PestsAndDiseases;
            existingCropMaster.MoleculesToAdd = updatedCropMasterDto.MoleculesToAdd;
            existingCropMaster.UpdatedBy = actor;

            await _cropMasterService.UpdateAsync(existingCropMaster);
            return NoContent();
        }

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{cropId}")]
        public async Task<IActionResult> Delete(string cropId)
        {
            var existingCropMaster = await _cropMasterService.GetByCropIdAsync(cropId);

            if (existingCropMaster is null)
            {
                return NotFound();
            }

            await _cropMasterService.RemoveAsync(cropId);

            return NoContent();
        }
    }
}