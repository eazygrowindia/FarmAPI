using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FarmAPI.Models
{
    public class Maintainer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("maintainerId")]
        public string MaintainerId { get; set; }

        [BsonElement("farmOwnerId")]
        public string FarmOwnerId { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }  //points to the user collection

        /// <summary>
        /// Farm Assistant's name
        /// </summary>
        [BsonElement("name")]
        public string MaintainerName { get; set; } = null!;

        [BsonElement("contactNumber")]
        public string ContactNumber { get; set; } = null!;

        [BsonElement("alternateContactNumber")]
        public string? AlternateContactNumber { get; set; }

        /// <summary>
        /// 10th, PUC/12th, Graduate, Post Graduate etc
        /// </summary>
        [BsonElement("education")]
        public string Education { get; set; } = null!;

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        /// <summary>
        /// Govt. provided identity proof document like Aadhar, Voter ID, Driving License etc
        /// </summary>
        [BsonElement("identityProofDocument")]
        public required string IdentityProofDocument { get; set; } = null!;

        /// <summary>
        /// Govt. provided identity proof number for the identity document selected above
        /// </summary>
        [BsonElement("identityProofNumber")]
        public required string IdentityProofNumber { get; set; } = null!;

        /// <summary>
        /// url pointing to the uploaded training certificate document for the maintainer/assistant
        /// </summary>
        [BsonElement("trainingCertificateUrl")]
        public string TrainingCertificateUrl { get; set; }

        /// <summary>
        /// List of Farm IDs maintained by the maintainer/assitant
        /// </summary>
        //[BsonElement("farmsMaintained")]
        //public List<string> FarmsMaintained { get; set; }

        /// <summary>
        /// this determines the access level of the maintainer/assitant in the system
        /// </summary>
        //[BsonElement("role")]
        //public string Role { get; set; } = null!;

        /// <summary>
        /// denotes whether the record is Active/Inactive in the system
        /// </summary>
        [BsonElement("systemStatus")]
        public string SystemStatus { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MaintainerPartial
    {
        [BsonElement("maintainerId")]
        public string MaintainerId { get; set; }
        
        [BsonElement("name")]
        public string MaintainerName { get; set; }

        public string Id => MaintainerId;
        public string Name => MaintainerName;
    }
}