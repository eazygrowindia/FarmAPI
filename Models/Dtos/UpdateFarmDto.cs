using System.Text.Json.Serialization;

namespace FarmAPI.Models.Dtos
{
    // Update DTO intentionally excludes the database-generated `Id` and the stable `FarmId`.
    // Use the route `id` (FarmId) when calling update endpoints to identify the resource.
    public class UpdateFarmDto
    {
        public string FarmName { get; set; } = null!;
        public string SurveyNumber { get; set; } = null!;
        public FarmAddress Address { get; set; }
        public List<string> Crops { get; set; }
        public double ShadeNetArea { get; set; }
        //public LatLong GeoTag { get; set; }
        public double FarmPondVolume { get; set; }
        public bool? IsSolarPowerAvailable { get; set; }
        public string? MotorCapacity { get; set; }
        public string AdditionalWaterSource { get; set; }

        private string? _waterTestCertificateUrl;

        [JsonPropertyName("waterTestCertificateUrl")]
        public string? WaterTestCertificateUrl
        {
            get => _waterTestCertificateUrl;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _waterTestCertificateUrl = value;
            }
        }

        [JsonPropertyName("waterTestCertificate")]
        public string? WaterTestCertificate
        {
            get => _waterTestCertificateUrl;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _waterTestCertificateUrl = value;
            }
        }

        [JsonPropertyName("waterCertificate")]
        public string? WaterCertificate
        {
            get => _waterTestCertificateUrl;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _waterTestCertificateUrl = value;
            }
        }

        [JsonPropertyName("waterCertificateUrl")]
        public string? WaterCertificateUrl
        {
            get => _waterTestCertificateUrl;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    _waterTestCertificateUrl = value;
            }
        }

        public bool? IsSinglePhasePower { get; set; }
        public bool? IsThreePhasePower { get; set; }
        //public PowerTimeSpan GridPowerUnAvailability { get; set; }
        public double? AutomationRoomSize { get; set; }
        //public FarmhouseNote FarmhouseNote { get; set; }
        public string StorageAreaNote { get; set; }
        //public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}