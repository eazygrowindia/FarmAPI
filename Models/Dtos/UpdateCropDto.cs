using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models.Dtos
{
    public class UpdateCropDto
    {
        public required string CropName { get; set; } = null!;
        public required double CropArea { get; set; }
        public required DateTime DateOfSowing { get; set; }
        public string ExpectedYield { get; set; }
    }
}