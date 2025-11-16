using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class Owner
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string OwnerName { get; set; } = null!;

        [BsonElement("identityProofDocument")]
        public required string IdentityProofDocument { get; set; } = null!;

        [BsonElement("identityProofNumber")]
        public required string IdentityProofNumber { get; set; } = null!;

        [BsonElement("contactNumber")]
        public string ContactNumber { get; set; } = null!;

        [BsonElement("alternateContactNumber")]
        public string AlternateContactNumber { get; set; } = null!;

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        [BsonElement("farmsOwned")]
        public List<string> FarmsOwned { get; set; }

        [BsonElement("systemStatus")]
        public string SystemStatus { get; set; } = null!;
    }
}