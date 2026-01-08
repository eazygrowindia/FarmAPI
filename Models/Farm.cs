using FarmAPI.Models.Dtos;
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

        [BsonElement("farmMaintainerId")]
        public string FarmMaintainerId { get; set; }

        [BsonElement("farmOwnerId")]
        public string FarmOwnerId { get; set; }

        [BsonElement("address")]
        public FarmAddress Address { get; set; } = null!;

        //[BsonElement("crops")]
        //public List<string> Crops { get; set; }

        /// <summary>
        /// Area covered under shade net in square meters
        /// </summary>
        [BsonElement("shadeNetArea")]
        public double ShadeNetArea { get; set; }

        [BsonElement("geoLocation")]
        public LatLong GeoLocation { get; set; } = null!;

        [BsonElement("weatherData")]
        public HistoricalWeather WeatherData { get; set; } = null!;

        /// <summary>
        /// In Liters or Cubic meter
        /// </summary>
        [BsonElement("farmPondVolume")]
        public double FarmPondVolume { get; set; }

        [BsonElement("isSolarPowerAvailable")]
        public bool? IsSolarPowerAvailable { get; set; }

        /// <summary>
        /// in terms of horse power
        /// 1 of the 3 options 1) 5hp 2) 7.5hp 3) 10hp
        /// </summary>
        [BsonElement("motorCapacity")]
        public string MotorCapacity { get; set; }

        [BsonElement("additionalWaterSource")]
        public string AdditionalWaterSource { get; set; }

        /// <summary>
        /// Upload water test certificate
        /// </summary>
        [BsonElement("waterTestCertificateUrl")]
        public string? WaterTestCertificateUrl { get; set; }

        [BsonElement("isSinglePhasePower")]
        public bool? IsSinglePhasePower { get; set; }

        [BsonElement("isThreePhasePower")]
        public bool? IsThreePhasePower { get; set; }

        [BsonElement("gridPowerUnAvailability")]
        public PowerTimeSpan GridPowerUnAvailability { get; set; }

        /// <summary>
        /// In squarefeet
        /// </summary>
        [BsonElement("automationRoomSize")]
        public double? AutomationRoomSize { get; set; }

        [BsonElement("farmhouseNote")]
        public FarmhouseNote FarmhouseNote { get; set; }

        [BsonElement("storageAreaNote")]
        public string StorageAreaNote { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    public class FarmAddress
    {
        public string? Pincode { get; set; } = null;
        public string State { get; set; }
        public string District { get; set; }
        public string? SubDistrict { get; set; } = null;
        public string Village { get; set; }
        public string? AddressLine { get; set; } = null;
        public string? Taluka { get; set; } = null;
        public string? Hobli { get; set; } = null;
        public string? SurveyNumber { get; set; } = null;
        public string? Hissa { get; set; } = null;
    }
}