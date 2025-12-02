using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class CropService
    {
        private readonly IMongoCollection<Crop> _cropCollection;

        public CropService(
            IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                farmGrowDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                farmGrowDatabaseSettings.Value.DatabaseName);

            _cropCollection = mongoDatabase.GetCollection<Crop>(
                farmGrowDatabaseSettings.Value.CropCollectionName);
        }

        public async Task<List<Crop>> GetAsync() =>
            await _cropCollection.Find(_ => true).ToListAsync();

        public async Task<Crop?> GetAsync(string id) =>
            await _cropCollection.Find(x => x.CropId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Crop newCrop) => 
            await _cropCollection.InsertOneAsync(newCrop);

        public async Task UpdateAsync(string id, Crop updatedCrop) =>
            await _cropCollection.ReplaceOneAsync(x => x.CropId == id, updatedCrop);

        public async Task RemoveAsync(string id) =>
            await _cropCollection.DeleteOneAsync(x => x.CropId == id);
    }
}