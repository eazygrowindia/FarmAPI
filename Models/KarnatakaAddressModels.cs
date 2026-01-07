using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace FarmAPI.Models
{
    public class TalukaResponse
    {
        public int TalukaCode { get; set; }
        public string TalukaName { get; set; }
    }

    public class HobliResponse
    {
        public int HobliCode { get; set; }

        public string HobliName { get; set; }
    }

    public class DistrictItem { public int DistrictCode { get; set; } public string DistrictName { get; set; } = string.Empty; }
    //public class TalukaItem { public int TALUKA_CODE { get; set; } public string TALUKA_NAME { get; set; } = string.Empty; }
    //public class HobliItem { public int HOBLI_CODE { get; set; } public string HOBLI_NAME { get; set; } = string.Empty; }
    //public class VillageItem { public int VILLAGE_CODE { get; set; } public string VILLAGE_NAME { get; set; } = string.Empty; }

    public class TalukaItem
    {
        [JsonPropertyName("TALUKA_CODE")]
        public int TALUKA_CODE { get; set; }

        [JsonPropertyName("TALUKA_NAME")]
        public string TALUKA_NAME { get; set; } = string.Empty;
    }

    public class HobliItem
    {
        [JsonPropertyName("HOBLI_CODE")]  // Adjust based on response
        public int HOBLI_CODE { get; set; }

        [JsonPropertyName("HOBLI_NAME")]
        public string HOBLI_NAME { get; set; } = string.Empty;
    }

    public class VillageItem  // Typo fix: "villege" → "village" in URL too?
    {
        [JsonPropertyName("VILLAGE_CODE")]
        public int VILLAGE_CODE { get; set; }

        [JsonPropertyName("VILLAGE_NAME")]
        public string VILLAGE_NAME { get; set; } = string.Empty;
    }

    public class KarnatakaLocation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int StateCode { get; set; }
        public string StateName { get; set; } = string.Empty;
        public int DistrictCode { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int TalukaCode { get; set; }
        public string TalukaName { get; set; } = string.Empty;
        public int HobliCode { get; set; }
        public string HobliName { get; set; } = string.Empty;
        public long VillageCode { get; set; }
        public string VillageName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string SourceStateFile { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? FirstImported { get; set; }
        //public DateTime? LastSynced { get; set; }
    }

}
