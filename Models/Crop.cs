using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class Crop
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public required string CropName { get; set; }

        [BsonElement("cropId")]
        public required string CropId { get; set; }

        [BsonElement("farmId")]
        public required string FarmId { get; set; }

        [BsonElement("cropMasterId")]
        public required string CropMasterId { get; set; }

        //[BsonElement("farmId")]
        //public required string FarmId { get; set; }

        /// <summary>
        /// Area covered under shade net in square meters
        /// </summary>
        [BsonElement("area")]
        public required double CropArea { get; set; }

        [BsonElement("dateOfSowing")]
        public required DateTime DateOfSowing { get; set; }

        [BsonElement("expectedYield")]
        public double ExpectedYield { get; set; }

        [BsonElement("probableHarvestDate")]
        public DateTime ProbableHarvestDate { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}