using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FarmAPI.Models
{
    public class FarmNote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }

        [BsonElement("noteId")]
        public string NoteId { get; set; } = null!;

        [BsonElement("farmId")]
        public required string FarmId { get; set; }

        [BsonElement("note")]
        public required string Note { get; set; }

        [BsonElement("createdBy")]
        public string? CreatedBy { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
