using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class HealthCheck
    {
        [BsonElement("healthReportUrl")]
        public string HealthReportUrl { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
