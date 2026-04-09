using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class CreateDiseaseControlInventoryDto
    {
        public required string FarmId { get; set; }
        public DateTime SuppliedDate { get; set; }
        public List<CreateDiseaseControlInventoryItemDto> DiseaseControlItems { get; set; } = new List<CreateDiseaseControlInventoryItemDto>();
        public required string Supplier { get; set; }
        public required string InvoiceNumber { get; set; }
    }

    public class CreateDiseaseControlInventoryItemDto
    {
        public required string DiseaseControlName { get; set; }
        public double QuantitySupplied { get; set; }
        public string QuantityMetric { get; set; }
    }
}