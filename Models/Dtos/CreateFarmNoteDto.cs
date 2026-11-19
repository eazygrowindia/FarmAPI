using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class CreateFarmNoteDto
    {
        public string? NoteId { get; set; }

        [Required]
        public required string FarmId { get; set; }

        [Required]
        public required string Note { get; set; }

        public string? CreatedBy { get; set; }
    }
}
