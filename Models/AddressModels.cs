using CsvHelper.Configuration.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmAPI.Models
{
    public class LgdLocationRow
    {
        public int stateCode { get; set; }
        public string stateNameEnglish { get; set; }
        public int districtCode { get; set; }
        public string districtNameEnglish { get; set; }
        public int subdistrictCode { get; set; }
        public string subdistrictNameEnglish { get; set; }
        public long villageCode { get; set; }
        public string villageNameEnglish { get; set; }
        public string pincode { get; set; }
    }

    public class LgdLocation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int StateCode { get; set; }
        public string StateName { get; set; } = string.Empty;
        public int DistrictCode { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int SubdistrictCode { get; set; }
        public string SubdistrictName { get; set; } = string.Empty;
        public long VillageCode { get; set; }
        public string VillageName { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string SourceStateFile { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        //public DateTime? FirstImported { get; set; }
        //public DateTime? LastSynced { get; set; }
    }


    //public class LgdLocation
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public ObjectId Id { get; set; }

    //    [Name("stateCode"), Index(0)]
    //    public int StateCode { get; set; }

    //    [Name("stateNameEnglish"), Index(1)]
    //    public string StateNameEnglish { get; set; } = string.Empty;

    //    [Name("districtCode"), Index(2)]
    //    public int DistrictCode { get; set; }

    //    [Name("districtNameEnglish"), Index(3)]
    //    public string DistrictNameEnglish { get; set; } = string.Empty;

    //    [Name("subdistrictCode"), Index(4)]
    //    public int SubdistrictCode { get; set; }

    //    [Name("subdistrictNameEnglish"), Index(5)]
    //    public string SubdistrictNameEnglish { get; set; } = string.Empty;

    //    [Name("villageCode"), Index(6)]
    //    public int VillageCode { get; set; }

    //    [Name("villageNameEnglish"), Index(7)]
    //    public string VillageNameEnglish { get; set; } = string.Empty;

    //    [Name("pincode"), Index(8)]
    //    public int Pincode { get; set; }
    //}

    public class PincodeResponse
    {
        public StateInfo State { get; set; } = new();
        public List<DistrictInfo> Districts { get; set; } = new();
        public List<SubdistrictInfo> Subdistricts { get; set; } = new();
        public List<VillageInfo> Villages { get; set; } = new();
    }

    public class StateInfo
    {
        public int StateCode { get; set; }
        public string StateName { get; set; } = string.Empty;

        //public override bool Equals(object? obj) =>
        //obj is StateInfo other && StateCode == other.StateCode && StateName == other.StateName;

        //public override int GetHashCode() => HashCode.Combine(StateCode, StateName);

        //public static bool operator ==(StateInfo? left, StateInfo? right) =>
        //    EqualityComparer<StateInfo>.Default.Equals(left, right);
        //public static bool operator !=(StateInfo? left, StateInfo? right) =>
        //    !(left == right);
    }

    public class DistrictInfo
    {
        public int DistrictCode { get; set; }
        public string DistrictName { get; set; } = string.Empty;
    }

    public class SubdistrictInfo
    {
        public int SubdistrictCode { get; set; }
        public string SubdistrictName { get; set; } = string.Empty;
    }

    public class SubdistrictResponse
    {
        public int SubDistrictCode { get; set; }
        public string SubDistrictName { get; set; } = string.Empty;
    }

    public class VillageInfo
    {
        public long VillageCode { get; set; }
        public string VillageName { get; set; } = string.Empty;
    }
}
