using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class CropMaster
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("cropId")]
        public required string CropId { get; set; }

        [BsonElement("cropName")]
        public required string CropName { get; set; }

        [BsonElement("durationInDays")]
        public int Duration { get; set; }

        [BsonElement("expectedYieldKgsPerSqmt")]
        public double ExpectedYield { get; set; }

        [BsonElement("sowingTime")]
        public string SowingTime { get; set; }

        [BsonElement("harvestTime")]
        public string HarvestTime { get; set; }

        [BsonElement("sowingMethod")]
        public string SowingMethod { get; set; }

        [BsonElement("pestsAndDiseases")]
        public string? PestsAndDiseases { get; set; }

        [BsonElement("moleculesToAdd")]
        public string? MoleculesToAdd { get; set; }

        /// <summary>
        /// References UserId from user collection
        /// </summary>
        [BsonElement("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// References UserId from user collection
        /// </summary>
        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}