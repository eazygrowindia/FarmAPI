using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class CropPartial
    {
        [BsonElement("name")]
        public string CropName { get; set; }

        [BsonElement("cropId")]
        public string CropId { get; set; }
    }
}
