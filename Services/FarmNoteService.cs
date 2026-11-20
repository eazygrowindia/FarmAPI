using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FarmNoteService
    {
        private readonly IMongoCollection<FarmNote> _farmNotesCollection;

        public FarmNoteService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _farmNotesCollection = db.FarmGrow.GetCollection<FarmNote>(settings.Value.FarmNotesCollectionName);
        }

        private FilterDefinition<FarmNote> BuildNoteIdentifierFilter(string noteId)
        {
            if (ObjectId.TryParse(noteId, out _))
            {
                return Builders<FarmNote>.Filter.Or(
                    Builders<FarmNote>.Filter.Eq(x => x.NoteId, noteId),
                    Builders<FarmNote>.Filter.Eq(x => x.Id, noteId)
                );
            }

            return Builders<FarmNote>.Filter.Eq(x => x.NoteId, noteId);
        }

        public async Task<List<FarmNote>> GetAsync() =>
            await _farmNotesCollection.Find(_ => true).ToListAsync();

        public async Task<FarmNote?> GetByNoteIdAsync(string noteId)
        {
            var filter = BuildNoteIdentifierFilter(noteId);
            return await _farmNotesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<FarmNote>> GetByFarmIdAsync(string farmId) =>
            await _farmNotesCollection.Find(x => x.FarmId == farmId).ToListAsync();

        public async Task CreateAsync(FarmNote newNote) =>
            await _farmNotesCollection.InsertOneAsync(newNote);

        public async Task UpdateAsync(string noteId, FarmNote updatedNote)
        {
            var filter = BuildNoteIdentifierFilter(noteId);

            var update = Builders<FarmNote>.Update
                .Set(x => x.FarmId, updatedNote.FarmId)
                .Set(x => x.Note, updatedNote.Note)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _farmNotesCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveAsync(string noteId)
        {
            var filter = BuildNoteIdentifierFilter(noteId);
            await _farmNotesCollection.DeleteOneAsync(filter);
        }
    }
}
