using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class CreateFertilizerInventoryDto
    {
        public required string FarmId { get; set; }
        public required string FertilizerName { get; set; }
        public double QuantitySupplied { get; set; }
        public DateTime SuppliedDate { get; set; }
    }
}