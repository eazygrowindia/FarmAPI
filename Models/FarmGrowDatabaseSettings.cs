namespace FarmAPI.Models
{
    public class FarmGrowDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = "";
        public string MagicLinksCollectionName { get; set; } = "";
        public string OtpCollectionName { get; set; } = "";
        public string OwnerCollectionName { get; set; } = null!;
        public string FarmCollectionName { get; set; } = null!;
        public string MaintainerCollectionName { get; set; } = null!;
        public string CropCollectionName { get; set; } = null!;
        public string ActivityCollectionName { get; set; } = null!;
        public string ObservationCollectionName { get; set; } = null!;
        public string CropMasterCollectionName { get;set; } = null!;
        public string FertilizerInventoryCollectionName { get; set; } = null!;
        public string DiseaseControlInventoryCollectionName { get; set; } = null!;
    }
}
