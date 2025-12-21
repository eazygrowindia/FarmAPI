using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{

    public class WebAuthnCredential
    {
        [BsonElement("credentialId")]
        public string CredentialId { get; set; } = "";

        [BsonElement("publicKey")]
        public string PublicKey { get; set; } = "";

        [BsonElement("signCount")]
        public long SignCount { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastUsedAt")]
        public DateTime? LastUsedAt { get; set; }
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = "";

        [BsonElement("mobile")]
        public string Mobile { get; set; } = "";

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("emailVerified")]
        public bool EmailVerified { get; set; }

        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        [BsonElement("passwordSalt")]
        public string? PasswordSalt { get; set; }

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new();   // "Owner", "Maintainer", etc.

        [BsonElement("webauthnCredentials")]
        public List<WebAuthnCredential> WebAuthnCredentials { get; set; } = new();

        [BsonElement("systemStatus")]
        public string SystemStatus { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }
    }
}