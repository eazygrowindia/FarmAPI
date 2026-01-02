using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace FarmAPI.Models
{
    public class FertilizerInventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("inventoryId")]
        public required string InventoryId { get; set; }

        [BsonElement("farmId")]
        public required string FarmId { get; set; }

        [BsonElement("fertilizerName")]
        public required string FertilizerName { get; set; }

        [BsonElement("quantitySupplied")]
        public double QuantitySupplied { get; set; }

        [BsonElement("suppliedDate")]
        public DateTime SuppliedDate { get; set; }

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