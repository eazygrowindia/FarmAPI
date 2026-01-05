namespace FarmAPI.Models
{
    public class AddressLoaderDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;
        public string LgdLocationsCollectionName { get; set; }
        public string KarnatakaLocationsCollectionName { get; set; }
        public string KarnatakaBhoomiDistrictMappingCollectionName { get; set; }
        public string KarnatakaStateName { get; set; }
    }
}
