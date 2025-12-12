using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class Summary
    {
        [BsonElement("totalFarms")]
        public int TotalFarms { get; set; }

        [BsonElement("totalCrops")]
        public int TotalCrops { get; set; }
    }
}