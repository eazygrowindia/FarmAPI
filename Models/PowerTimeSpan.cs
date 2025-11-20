using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class PowerTimeSpan
    {
        [BsonElement("duration")]
        public double Duration { get; set; }

        [BsonElement("timeSpan")]
        public List<TimeLifeSpan> TimeSpan { get; set; }
    }

    public class TimeLifeSpan
    {
        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }
    }
}