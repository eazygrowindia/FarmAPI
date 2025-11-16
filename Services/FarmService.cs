using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FarmService
    {
        private readonly IMongoCollection<Farm> _farmCollection;

        public FarmService(
            IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                farmGrowDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                farmGrowDatabaseSettings.Value.DatabaseName);

            _farmCollection = mongoDatabase.GetCollection<Farm>(
                farmGrowDatabaseSettings.Value.FarmCollectionName);
        }

        public async Task<List<Farm>> GetAsync() =>
            await _farmCollection.Find(_ => true).ToListAsync();

        public async Task<Farm?> GetAsync(string id) =>
            await _farmCollection.Find(x => x.FarmId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Farm newFarm) => 
            await _farmCollection.InsertOneAsync(newFarm);

        public async Task UpdateAsync(string id, Farm updatedFarm) =>
            await _farmCollection.ReplaceOneAsync(x => x.FarmId == id, updatedFarm);

        public async Task RemoveAsync(string id) =>
            await _farmCollection.DeleteOneAsync(x => x.FarmId == id);
    }
}