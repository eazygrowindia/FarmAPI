using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class UpdateDiseaseControlInventoryDto
    {
        public required string DiseaseControlName { get; set; }
        public double QuantitySupplied { get; set; }
        public DateTime SuppliedDate { get; set; }
        public double QuantityUsed { get; set; }
        public DateTime UsedDate { get; set; }
    }
}