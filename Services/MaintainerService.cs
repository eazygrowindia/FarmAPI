using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class MaintainerService
    {
        private readonly IMongoCollection<Maintainer> _maintainerCollection;

        public MaintainerService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _maintainerCollection = db.FarmGrow.GetCollection<Maintainer>(settings.Value.MaintainerCollectionName);
        }

        public async Task<List<Maintainer>> GetAsync() =>
            await _maintainerCollection.Find(_ => true).ToListAsync();

        public async Task<List<MaintainerPartial>> GetAllFarmOwnersNameAsync(string ownerId)
        {
            var projections = Builders<Maintainer>.Projection
                .Include(o => o.MaintainerId)
                .Include(o => o.MaintainerName)
                .Exclude("_id");

            var maintainers = await _maintainerCollection.Find(_ => true).Project<MaintainerPartial>(projections).ToListAsync();
            return maintainers;
        }

        public async Task<Maintainer?> GetAsync(string id) =>
            await _maintainerCollection.Find(x => x.MaintainerId == id).FirstOrDefaultAsync();

        public async Task<Maintainer?> GetAsyncByMobile(string mobile) =>
            await _maintainerCollection.Find(x => x.ContactNumber.ToLower() == mobile.ToLower()).FirstOrDefaultAsync();

        public async Task CreateAsync(Maintainer newMaintainer) =>
            await _maintainerCollection.InsertOneAsync(newMaintainer);

        public async Task UpdateAsync(string id, Maintainer updatedMaintainer) =>
            await _maintainerCollection.ReplaceOneAsync(x => x.MaintainerId == id, updatedMaintainer);

        public async Task UpdateAsyncByFilterUpdateDefinitions(FilterDefinition<Maintainer>? filter, UpdateDefinition<Maintainer>? update)
        {
            await _maintainerCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveAsync(string id) =>
            await _maintainerCollection.DeleteOneAsync(x => x.MaintainerId == id);
    }
}