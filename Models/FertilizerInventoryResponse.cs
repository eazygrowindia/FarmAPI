namespace FarmAPI.Models
{
    public class FertilizerInventoryResponse
    {
        public string FarmId { get; set; }
        public string InventoryId { get; set; }
        public DateTime SuppliedDate { get; set; }
        public List<FertilizerInventoryItem> FertilizerItems { get; set; } = new List<FertilizerInventoryItem>();
        public string Supplier { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
