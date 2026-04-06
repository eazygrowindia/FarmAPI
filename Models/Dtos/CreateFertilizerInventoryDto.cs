using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class CreateFertilizerInventoryDto
    {
        public required string FarmId { get; set; }
        public DateTime SuppliedDate { get; set; }
        public List<CreateFertilizerInventoryItemDto> FertilizerItems { get; set; } = new List<CreateFertilizerInventoryItemDto>();
        public required string Supplier { get; set; }
        public required string InvoiceNumber { get; set; }
    }

    public class CreateFertilizerInventoryItemDto
    {
        public required string FertilizerName { get; set; }
        public double QuantitySupplied { get; set; }
        public string QuantityMetric { get; set; }
    }
}