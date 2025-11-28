namespace FarmAPI.Models.Dtos
{
    // Update DTO intentionally excludes the database-generated `Id` and the stable `FarmId`.
    // Use the route `id` (FarmId) when calling update endpoints to identify the resource.
    public class UpdateFarmDto
    {
        public string FarmName { get; set; } = null!;
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
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}