using FarmAPI.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace FarmAPI.Models
{
    public class HistoricalWeather
    {
        [BsonElement("weatherRangeStart")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly StartDate { get; set; }

        [BsonElement("weatherRangeEnd")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly EndDate { get; set; }

        [BsonElement("monthly")]
        public List<MonthlyWeatherMeans> Monthly { get; set; }

        [BsonElement("rainyMonths")]
        public List<string> RainyMonths { get; set; }

        [BsonElement("winterMonths")]
        public List<string> WinterMonths { get; set; }
    }

    public class MonthlyWeatherMeans
    {
        [BsonElement("month")]
        public string Month { get; set; }

        [BsonElement("meanTemp")]
        public double MeanTemp { get; set; }

        [BsonElement("totalRain")]
        public double TotalRain { get; set; }

        [BsonElement("meanWind")]
        public double MeanWind { get; set; }
    }
}