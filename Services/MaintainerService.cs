using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class MaintainerService
    {
        private readonly IMongoCollection<Maintainer> _maintainerCollection;

        public MaintainerService(
            IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                farmGrowDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                farmGrowDatabaseSettings.Value.DatabaseName);

            _maintainerCollection = mongoDatabase.GetCollection<Maintainer>(
                farmGrowDatabaseSettings.Value.MaintainerCollectionName);
        }

        public async Task<List<Maintainer>> GetAsync() =>
            await _maintainerCollection.Find(_ => true).ToListAsync();

        public async Task<Maintainer?> GetAsync(string id) =>
            await _maintainerCollection.Find(x => x.MaintainerId == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Maintainer newMaintainer) =>
            await _maintainerCollection.InsertOneAsync(newMaintainer);

        public async Task UpdateAsync(string id, Maintainer updatedMaintainer) =>
            await _maintainerCollection.ReplaceOneAsync(x => x.MaintainerId == id, updatedMaintainer);

        public async Task RemoveAsync(string id) =>
            await _maintainerCollection.DeleteOneAsync(x => x.MaintainerId == id);
    }
}