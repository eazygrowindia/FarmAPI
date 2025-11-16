using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class OwnerService
    {
        private readonly IMongoCollection<Owner> _ownerCollection;

        public OwnerService(
            IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                farmGrowDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                farmGrowDatabaseSettings.Value.DatabaseName);

            _ownerCollection = mongoDatabase.GetCollection<Owner>(
                farmGrowDatabaseSettings.Value.OwnerCollectionName);
        }

        public async Task<List<Owner>> GetAsync() =>
            await _ownerCollection.Find(_ => true).ToListAsync();

        public async Task<Owner?> GetAsync(string id) =>
            await _ownerCollection.Find(x => x.OwnerId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Owner newOwner) =>
            await _ownerCollection.InsertOneAsync(newOwner);

        public async Task UpdateAsync(string id, Owner updatedOwner) =>
            await _ownerCollection.ReplaceOneAsync(x => x.OwnerId == id, updatedOwner);

        public async Task RemoveAsync(string id) =>
            await _ownerCollection.DeleteOneAsync(x => x.OwnerId == id);
    }
}