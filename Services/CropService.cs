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

        public async Task UpdateAsync(string id, Crop updatedCrop)
        {
            var filter = Builders<Crop>.Filter.Eq(x => x.CropId, id);
            await _cropCollection.UpdateOneAsync(
                filter,
                Builders<Crop>.Update
                    .Set(c => c.CropName, updatedCrop.CropName)
                    .Set(c => c.CropArea, updatedCrop.CropArea)
                    .Set(c => c.DateOfSowing, updatedCrop.DateOfSowing)
                    .Set(c => c.ExpectedYield, updatedCrop.ExpectedYield)
                    .Set(c => c.ProbableHarvestDate, updatedCrop.ProbableHarvestDate)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string id) =>
            await _cropCollection.DeleteOneAsync(x => x.CropId == id);
    }
}