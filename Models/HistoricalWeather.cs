using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class HistoricalWeather
    {
        [BsonElement("meanTemperature")]
        public double MeanTemperature { get; set; }

        [BsonElement("winterMonths")]
        public double WinterMonths { get; set; }

        [BsonElement("rainyMonths")]
        public double RainyMonths { get; set; }

        [BsonElement("rainfall")]
        public double Rainfall { get; set; }

        [BsonElement("windSpeed")]
        public double WindSpeed { get; set; }
    }
}