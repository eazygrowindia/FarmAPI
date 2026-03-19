using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("activityId")]
        public required string ActivityId { get; set; }

        [BsonElement("cropId")]
        public required string CropId { get; set; }

        [BsonElement("type")]
        public required string ActivityType { get; set; }

        [BsonElement("message")]
        public required string Message { get; set; }

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}