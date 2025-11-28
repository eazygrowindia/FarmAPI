using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FarmAPI.Models.Dtos
{
    public class CreateFarmDto
    {
        public string FarmName { get; set; } = null!;
        public string FarmId { get; set; } = null!;
        public string SurveyNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public double ShadeNetArea { get; set; }
        //public LatLong GeoTag { get; set; }
        public double FarmPondVolume { get; set; }
        public bool? IsSolarPowerAvailable { get; set; }
        public string? MotorCapacity { get; set; }
        public string AdditionalWaterSource { get; set; }
        public string? WaterTestCertificateUrl { get; set; }
        public bool? IsSinglePhasePower { get; set; }
        public bool? IsThreePhasePower { get; set; }
        //public PowerTimeSpan GridPowerUnAvailability { get; set; }
        public double? AutomationRoomSize { get; set; }
        //public FarmhouseNote FarmhouseNote { get; set; }
        public string StorageAreaNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}