using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class OtpSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("mobile")]
        public string Mobile { get; set; } = "";

        [BsonElement("otpHash")]
        public string OtpHash { get; set; } = "";

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("consumed")]
        public bool Consumed { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
