using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class FarmhouseNote
    {
        [BsonElement("hasBathroom")]
        public bool HasBathroom { get; set; }

        [BsonElement("hasKitchen")]
        public bool HasKitchen { get; set; }

        [BsonElement("stayingSpace")]
        public string StayingSpace { get; set; }
    }
}