namespace FarmAPI.Models
{
    public class AddressLoaderDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;
        public string LgdLocationsCollectionName { get; set; }
    }
}
