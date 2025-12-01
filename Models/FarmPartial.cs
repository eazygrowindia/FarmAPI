using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class FarmPartial
    {
        [BsonElement("name")]
        public string FarmName { get; set; } = null!;

        [BsonElement("farmId")]
        public string FarmId { get; set; }
    }
}
