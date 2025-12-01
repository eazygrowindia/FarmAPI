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
        public required string CropName { get; set; } = null!;

        [BsonElement("cropId")]
        public required string CropId { get; set; }

        [BsonElement("area")]
        public required string CropArea { get; set; } = null!;

        [BsonElement("dateOfSowing")]
        public required DateTime DateOfSowing { get; set; }

        /// <summary>
        /// Area covered under shade net in square meters
        /// </summary>
        [BsonElement("expectedYield")]
        public string ExpectedYield { get; set; }

        [BsonElement("probableHarvestDate")]
        public DateTime ProbableHarvestDate { get; set; }
    }
}