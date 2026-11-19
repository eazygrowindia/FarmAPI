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
    public class FarmNoteController : ControllerBase
    {
        private readonly FarmNoteService _farmNoteService;

        public FarmNoteController(FarmNoteService farmNoteService)
        {
            _farmNoteService = farmNoteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<FarmNote>>> Get() =>
            await _farmNoteService.GetAsync();

        [HttpGet("{noteId}")]
        public async Task<ActionResult<FarmNote>> GetByNoteId(string noteId)
        {
            var note = await _farmNoteService.GetByNoteIdAsync(noteId);

            if (note is null)
            {
                return NotFound();
            }

            return note;
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<List<FarmNote>>> GetByFarmId(string farmId)
        {
            var notes = await _farmNoteService.GetByFarmIdAsync(farmId);
            return notes;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmNoteDto newNoteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noteId = string.IsNullOrWhiteSpace(newNoteDto.NoteId)
                ? Guid.NewGuid().ToString()
                : newNoteDto.NoteId;

            var existingNote = await _farmNoteService.GetByNoteIdAsync(noteId);
            if (existingNote != null)
            {
                var problem = new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = $"A note with NoteId '{noteId}' already exists.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problem);
            }

            var newNote = new FarmNote
            {
                NoteId = noteId,
                FarmId = newNoteDto.FarmId,
                Note = newNoteDto.Note,
                CreatedBy = newNoteDto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _farmNoteService.CreateAsync(newNote);
            return CreatedAtAction(nameof(GetByNoteId), new { noteId = newNote.NoteId }, newNote);
        }

        [HttpPost("farm/{farmId}")]
        public async Task<IActionResult> CreateByFarmId(string farmId, [FromBody] CreateFarmNoteDto newNoteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var noteId = string.IsNullOrWhiteSpace(newNoteDto.NoteId)
                ? Guid.NewGuid().ToString()
                : newNoteDto.NoteId;

            var newNote = new FarmNote
            {
                NoteId = noteId,
                FarmId = farmId,
                Note = newNoteDto.Note,
                CreatedBy = newNoteDto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _farmNoteService.CreateAsync(newNote);
            return CreatedAtAction(nameof(GetByNoteId), new { noteId = newNote.NoteId }, newNote);
        }

        [HttpPut("{noteId}")]
        public async Task<IActionResult> Update(string noteId, [FromBody] UpdateFarmNoteDto updatedNoteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingNote = await _farmNoteService.GetByNoteIdAsync(noteId);
            if (existingNote == null)
            {
                return NotFound();
            }

            if (updatedNoteDto.FarmId != null) existingNote.FarmId = updatedNoteDto.FarmId;
            if (updatedNoteDto.Note != null) existingNote.Note = updatedNoteDto.Note;

            await _farmNoteService.UpdateAsync(noteId, existingNote);
            return NoContent();
        }

        [HttpPut("farm/{farmId}")]
        public async Task<IActionResult> UpdateByFarmId(string farmId, [FromBody] UpdateFarmNoteDto updatedNoteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingNotes = await _farmNoteService.GetByFarmIdAsync(farmId);
            if (existingNotes == null || existingNotes.Count == 0)
            {
                var newNote = new FarmNote
                {
                    NoteId = Guid.NewGuid().ToString(),
                    FarmId = farmId,
                    Note = updatedNoteDto.Note ?? "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _farmNoteService.CreateAsync(newNote);
                return Ok(newNote);
            }

            var existingNote = existingNotes.First();
            if (updatedNoteDto.Note != null) existingNote.Note = updatedNoteDto.Note;

            await _farmNoteService.UpdateAsync(existingNote.NoteId, existingNote);
            return NoContent();
        }

        [HttpPut("farm/{farmId}/{noteId}")]
        public async Task<IActionResult> UpdateByFarmIdAndNoteId(string farmId, string noteId, [FromBody] UpdateFarmNoteDto updatedNoteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingNote = await _farmNoteService.GetByNoteIdAsync(noteId);
            if (existingNote == null || existingNote.FarmId != farmId)
            {
                return NotFound();
            }

            if (updatedNoteDto.Note != null) existingNote.Note = updatedNoteDto.Note;

            await _farmNoteService.UpdateAsync(noteId, existingNote);
            return NoContent();
        }

        [HttpDelete("{noteId}")]
        public async Task<IActionResult> Delete(string noteId)
        {
            var existingNote = await _farmNoteService.GetByNoteIdAsync(noteId);

            if (existingNote is null)
            {
                return NotFound();
            }

            await _farmNoteService.RemoveAsync(noteId);
            return NoContent();
        }

        [HttpDelete("farm/{farmId}/{noteId}")]
        public async Task<IActionResult> DeleteByFarmIdAndNoteId(string farmId, string noteId)
        {
            var existingNote = await _farmNoteService.GetByNoteIdAsync(noteId);

            if (existingNote is null || existingNote.FarmId != farmId)
            {
                return NotFound();
            }

            await _farmNoteService.RemoveAsync(noteId);
            return NoContent();
        }
    }
}
