using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models.Dtos
{
    public class CreateCropDto
    {
        public required string CropName { get; set; } = null!;
        public required string CropId { get; set; }
        public required string CropArea { get; set; } = null!;
        public required DateTime DateOfSowing { get; set; }
        public string ExpectedYield { get; set; }
    }
}