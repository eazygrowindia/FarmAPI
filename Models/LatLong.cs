using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class LatLong
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longtitude")]
        public double Longitude { get; set; }
    }
}