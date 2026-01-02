using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FarmAPI.Models.Dtos
{
    public class CreateCropDto
    {
        [Required(ErrorMessage = "Crop name is required")]
        [MinLength(1, ErrorMessage = "Crop name cannot be empty")]
        public required string CropName { get; set; }
        public required string CropId { get; set; }
        public required string FarmId { get; set; }
        public required string CropMasterId { get; set; }
        public required double CropArea { get; set; }
        public required DateTime DateOfSowing { get; set; }
    }
}