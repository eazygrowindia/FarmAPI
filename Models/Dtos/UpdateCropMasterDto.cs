using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class UpdateCropMasterDto
    {
        public required string CropName { get; set; }
        public int Duration { get; set; }
        public double ExpectedYield { get; set; }
        public string SowingTime { get; set; }
        public string HarvestTime { get; set; }
        public string SowingMethod { get; set; }
        public string? PestsAndDiseases { get; set; }
        public string? MoleculesToAdd { get; set; }
    }
}