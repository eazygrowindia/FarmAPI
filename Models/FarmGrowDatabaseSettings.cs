namespace FarmAPI.Models
{
    public class FarmGrowDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string OwnerCollectionName { get; set; } = null!;
        public string FarmCollectionName { get; set; } = null!;
        public string MaintainerCollectionName { get; set; } = null!;
        public string CropCollectionName { get; set; } = null!;
        public string ActivityCollectionName { get; set; } = null!;
    }
}
