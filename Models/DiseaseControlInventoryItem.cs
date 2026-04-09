using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class DiseaseControlInventoryItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("inventoryId")]
        public required string InventoryId { get; set; }

        [BsonElement("inventoryItemId")]
        public required string InventoryItemId { get; set; }

        [BsonElement("diseaseControlName")]
        public required string DiseaseControlName { get; set; }

        [BsonElement("quantitySupplied")]
        public double QuantitySupplied { get; set; }

        [BsonElement("quantityMetric")]
        public string QuantityMetric { get; set; }

        [BsonElement("quantityUsed")]
        public double QuantityUsed { get; set; }

        [BsonElement("usedDate")]
        public DateTime UsedDate { get; set; }

        /// <summary>
        /// References UserId from user collection
        /// </summary>
        [BsonElement("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// References UserId from user collection
        /// </summary>
        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
