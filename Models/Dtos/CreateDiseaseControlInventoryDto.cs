using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class CreateDiseaseControlInventoryDto
    {
        public required string FarmId { get; set; }
        public required string FertilizerName { get; set; }
        public double QuantitySupplied { get; set; }
        public DateOnly SuppliedDate { get; set; }
        public double QuantityUsed { get; set; }
        public DateOnly UsedDate { get; set; }
    }
}