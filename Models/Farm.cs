using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class Farm
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string FarmName { get; set; } = null!;

        [BsonElement("farmId")]
        public string FarmId { get; set; }

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        [BsonElement("gpsLocation")]
        public string GPSLocation { get; set; } = null!;

        [BsonElement("sizeSqMtrs")]
        public string SizeInSqMtrs { get; set; } = null!;
    }
}