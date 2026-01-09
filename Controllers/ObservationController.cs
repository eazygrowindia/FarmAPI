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
    public class ObservationController : ControllerBase
    {
        private readonly ObservationService _observationService;

        public ObservationController(ObservationService observationService) =>
            _observationService = observationService;

        [HttpGet]
        public async Task<List<Observation>> Get() =>
            await _observationService.GetAsync();

        [HttpGet("{observationId}")]
        public async Task<ActionResult<Observation>> Get(string observationId)
        {
            var observation = await _observationService.GetByObservationIdAsync(observationId);

            if (observation is null)
            {
                return NotFound();
            }

            return observation;
        }

        [HttpGet("CropId/{cropId}")]
        public async Task<ActionResult<List<Observation>>> GetByCropId(string cropId)
        {
            var observations = await _observationService.GetByCropIdAsync(cropId);

            if (observations is null || observations.Count <= 0)
            {
                return NotFound();
            }

            return observations;
        }

        [HttpPost("AddObservation")]
        public async Task<IActionResult> Create([FromBody] CreateObservationDto newObservationDto)
        {
            // dto.ObservationName is guaranteed non-null/empty here
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // Returns 400 with errors

            var newObservation = new Observation
            {
                ObservationId = Guid.NewGuid().ToString(),
                CropId = newObservationDto.CropId,
                ObservationType = newObservationDto.ObservationType,
                Message = newObservationDto.Message,
                ImageUrl = newObservationDto.Photo,
                VoiceNoteUrl = newObservationDto.VoiceNote
            };

            var existingObservation = await _observationService.GetByObservationIdAsync(newObservation.ObservationId);
            if (existingObservation != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"An observation with ObservationId '{newObservation.ObservationId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path                 
                };
                return BadRequest(problem);
            }

            await _observationService.CreateAsync(newObservation);
            return CreatedAtAction(nameof(Get), new { id = newObservation.Id }, newObservation);
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(string id, [FromBody] UpdateObservationDto updatedObservationDto)
        //{
        //    // dto.ObservationName is guaranteed non-null/empty here
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);  // Returns 400 with errors

        //    //TODO - take a look at it when update is needed
        //    var existingObservation = await _observationService.GetByObservationIdAsync(id);
        //    if (existingObservation == null || existingObservation.CropId != updatedObservationDto.CropId) return NotFound();

        //    existingObservation.ObservationType = updatedObservationDto.ObservationType;
        //    existingObservation.Message = updatedObservationDto.Message;

        //    await _observationService.UpdateAsync(id, existingObservation);
        //    return NoContent();
        //}

        //[HttpDelete("{id:length(24)}")]
        [HttpDelete("{observationId}")]
        public async Task<IActionResult> Delete(string observationId)
        {
            var existingObservation = await _observationService.GetByObservationIdAsync(observationId);

            if (existingObservation is null)
            {
                return NotFound();
            }

            await _observationService.RemoveAsync(observationId);

            return NoContent();
        }
    }
}