using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class ActivityService
    {
        private readonly IMongoCollection<Activity> _activityCollection;

        public ActivityService(
            IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                farmGrowDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                farmGrowDatabaseSettings.Value.DatabaseName);

            _activityCollection = mongoDatabase.GetCollection<Activity>(
                farmGrowDatabaseSettings.Value.ActivityCollectionName);
        }

        public async Task<List<Activity>> GetAsync() =>
            await _activityCollection.Find(_ => true).ToListAsync();

        public async Task<Activity?> GetByActivityIdAsync(string id) =>
            await _activityCollection.Find(x => x.ActivityId == id).FirstOrDefaultAsync();

        public async Task<List<Activity>?> GetByCropIdAsync(string id) =>
            await _activityCollection.Find(x => x.CropId == id).ToListAsync();

        public async Task<long> GetTodayNewActivityCountAsync()
        {
            // Work in UTC; adjust if you need local time window
            var today = DateTime.UtcNow.Date;          // today 00:00 UTC
            var tomorrow = today.AddDays(1);           // next day 00:00 UTC

            var builder = Builders<Activity>.Filter;

            var filter = builder.Gte(a => a.CreatedAt, today) &
                         builder.Lt(a => a.CreatedAt, tomorrow);

            return await _activityCollection.CountDocumentsAsync(filter);
        }

        public async Task CreateAsync(Activity newActivity) => 
            await _activityCollection.InsertOneAsync(newActivity);

        public async Task UpdateAsync(string id, Activity updatedActivity)
        {
            var filter = Builders<Activity>.Filter.And(
                Builders<Activity>.Filter.Eq(c => c.ActivityId, id),
                Builders<Activity>.Filter.Eq(c => c.CropId, updatedActivity.CropId)
                );

            await _activityCollection.UpdateOneAsync(filter,
                Builders<Activity>.Update
                    .Set(c => c.ActivityType, updatedActivity.ActivityType)
                    .Set(c => c.Message, updatedActivity.Message)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string id) =>
            await _activityCollection.DeleteOneAsync(x => x.ActivityId == id);
    }
}