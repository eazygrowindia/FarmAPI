using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class CropService
    {
        private readonly IMongoCollection<Crop> _cropCollection;

        //public CropService(
        //    IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        //{
        //    var mongoClient = new MongoClient(
        //        farmGrowDatabaseSettings.Value.ConnectionString);

        //    var mongoDatabase = mongoClient.GetDatabase(
        //        farmGrowDatabaseSettings.Value.DatabaseName);

        //    _cropCollection = mongoDatabase.GetCollection<Crop>(
        //        farmGrowDatabaseSettings.Value.CropCollectionName);
        //}

        public CropService(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _cropCollection = db.GetCollection<Crop>(settings.Value.CropCollectionName);
        }

        public async Task<List<Crop>> GetAsync() =>
            await _cropCollection.Find(_ => true).ToListAsync();

        public async Task<long> GetDistinctCropsCountAsync()
        {
            var pipeline = _cropCollection.Aggregate()
                .Group(
                    key => key.CropId,
                    g => new { Id = g.Key }           // group by CropId
                )
                .Count();                             // count distinct groups

            var result = await pipeline.FirstOrDefaultAsync();
            return result?.Count ?? 0;
        }

        public async Task<Crop?> GetAsync(string id) =>
            await _cropCollection.Find(x => x.CropId == id).FirstOrDefaultAsync();

        public async Task<List<CropPartial>> GetPartialCropByIdOrName(string searchTerm)
        {
            var filter = Builders<Crop>.Filter.Or(
                Builders<Crop>.Filter.Regex(x => x.CropId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Crop>.Filter.Regex(x => x.CropName, new BsonRegularExpression(searchTerm, "i"))
            );

            var projection = Builders<Crop>.Projection.Include(f => f.CropId)
                                           .Include(f => f.CropName)
                                           .Exclude("_id"); // Optional to exclude 

            return await _cropCollection.Find(filter).Project<CropPartial>(projection).ToListAsync();
        }

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