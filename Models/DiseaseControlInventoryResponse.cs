namespace FarmAPI.Models
{
    public class DiseaseControlInventoryResponse
    {
        public string FarmId { get; set; }
        public string InventoryId { get; set; }
        public DateTime SuppliedDate { get; set; }
        public List<DiseaseControlInventoryItem> DiseaseControlItems { get; set; } = new List<DiseaseControlInventoryItem>();
        public string Supplier { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
