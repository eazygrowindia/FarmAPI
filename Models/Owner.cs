using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class Owner
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ownerId")]
        public string OwnerId { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }  //points to the user collection

        [BsonElement("name")]
        public string OwnerName { get; set; } = null!;

        [BsonElement("identityProofDocument")]
        public required string IdentityProofDocument { get; set; } = null!;

        [BsonElement("identityProofNumber")]
        public required string IdentityProofNumber { get; set; } = null!;

        [BsonElement("contactNumber")]
        public string ContactNumber { get; set; } = null!;

        [BsonElement("alternateContactNumber")]
        public string AlternateContactNumber { get; set; }

        [BsonElement("emailId")]
        public string? EmailId { get; set; }

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        //[BsonElement("farmsOwned")]
        //public List<string> FarmsOwned { get; set; }

        //[BsonElement("maintainers")]
        //public List<string> Maintainers { get; set; }

        [BsonElement("systemStatus")]
        public string SystemStatus { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OwnerPartial
    {
        public string OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;
    }
}