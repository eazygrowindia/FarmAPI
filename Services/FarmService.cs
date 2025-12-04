using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

        public async Task<List<FarmPartial>> GetPartialFarmByIdOrName(string searchTerm)
        {
            var filter = Builders<Farm>.Filter.Or(
                Builders<Farm>.Filter.Regex(x => x.FarmId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Farm>.Filter.Regex(x => x.FarmName, new BsonRegularExpression(searchTerm, "i"))
            );

            var projection = Builders<Farm>.Projection.Include(f => f.FarmId)
                                           .Include(f => f.FarmName)
                                           .Exclude("_id"); // Optional to exclude 

            return await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
        }

        public async Task<List<Farm>> GetFarmByIdOrName(string searchTerm)
        {
            var filter = Builders<Farm>.Filter.Or(
                Builders<Farm>.Filter.Regex(x => x.FarmId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Farm>.Filter.Regex(x => x.FarmName, new BsonRegularExpression(searchTerm, "i"))
            );

            return await _farmCollection.Find(filter).ToListAsync();
        }

        public async Task CreateAsync(Farm newFarm) => 
            await _farmCollection.InsertOneAsync(newFarm);

        public async Task UpdateAsync(string id, Farm updatedFarm)
        {
            //await _farmCollection.ReplaceOneAsync(x => x.FarmId == id, updatedFarm);
            var filter = Builders<Farm>.Filter.Eq(x => x.FarmId, id);

            await _farmCollection.UpdateOneAsync(
                filter,
                Builders<Farm>.Update
                    .Set(f => f.FarmName, updatedFarm.FarmName)
                    .Set(f => f.SurveyNumber, updatedFarm.SurveyNumber)
                    .Set(f => f.Address, updatedFarm.Address)
                    .Set(f => f.ShadeNetArea, updatedFarm.ShadeNetArea)
                    .Set(f => f.GeoTag, updatedFarm.GeoTag)
                    .Set(f => f.WeatherData, updatedFarm.WeatherData)
                    .Set(f => f.FarmPondVolume, updatedFarm.FarmPondVolume)
                    .Set(f => f.IsSolarPowerAvailable, updatedFarm.IsSolarPowerAvailable)
                    .Set(f => f.MotorCapacity, updatedFarm.MotorCapacity)
                    .Set(f => f.AdditionalWaterSource, updatedFarm.AdditionalWaterSource)
                    .Set(f => f.WaterTestCertificateUrl, updatedFarm.WaterTestCertificateUrl)
                    .Set(f => f.IsSinglePhasePower, updatedFarm.IsSinglePhasePower)
                    .Set(f => f.IsThreePhasePower, updatedFarm.IsThreePhasePower)
                    .Set(f => f.GridPowerUnAvailability, updatedFarm.GridPowerUnAvailability)
                    .Set(f => f.AutomationRoomSize, updatedFarm.AutomationRoomSize)
                    .Set(f => f.FarmhouseNote, updatedFarm.FarmhouseNote)
                    .Set(f => f.StorageAreaNote, updatedFarm.StorageAreaNote)
                    .Set(f => f.Crops, updatedFarm.Crops)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow)
            );
        }

        //public async Task FindAndUpdateAsync(string id, Farm updatedFarm)
        //{
        //    var filter = Builders<Farm>.Filter.Eq(x => x.FarmId, id);

        //    await _farmCollection.FindOneAndUpdateAsync(
        //        filter,
        //        Builders<Farm>.Update.Set(f => f.UpdatedAt, DateTime.UtcNow)
        //        );
        //}

        public async Task RemoveAsync(string id) =>
            await _farmCollection.DeleteOneAsync(x => x.FarmId == id);
    }
}