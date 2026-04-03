using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class OwnerService
    {
        private readonly IMongoCollection<Owner> _ownerCollection;
        public OwnerService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _ownerCollection = db.FarmGrow.GetCollection<Owner>(settings.Value.OwnerCollectionName);
        }

        public async Task<List<Owner>> GetAsync() =>
            await _ownerCollection.Find(_ => true).ToListAsync();

        public async Task<List<OwnerPartial>> GetAllFarmOwnersNameAsync()
        {
            var projections = Builders<Owner>.Projection
                .Include(o => o.OwnerId)
                .Include(o => o.OwnerName)
                .Exclude("_id");
            var owners = await _ownerCollection.Find(_ => true).Project<OwnerPartial>(projections).ToListAsync();
            return owners;
        }

        public async Task<Owner?> GetAsync(string id) =>
            await _ownerCollection.Find(x => x.OwnerId == id).FirstOrDefaultAsync();

        public async Task<Owner?> GetAsyncByName(string name) =>
            await _ownerCollection.Find(x => x.OwnerName.ToLower() == name.ToLower()).FirstOrDefaultAsync();

        public async Task<Owner?> GetAsyncByMobile(string mobile) =>
            await _ownerCollection.Find(x => x.ContactNumber.ToLower() == mobile.ToLower()).FirstOrDefaultAsync();

        public async Task<List<OwnerPartial>> SearchOwnersAsync(string searchTerm)
        {
            var projection = Builders<Owner>.Projection
                .Include(o => o.OwnerId)
                .Include(o => o.OwnerName)
                .Exclude("_id");

            var filter = Builders<Owner>.Filter.Or(
                Builders<Owner>.Filter.Regex(x => x.OwnerId, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Owner>.Filter.Regex(x => x.OwnerName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Owner>.Filter.Regex(x => x.ContactNumber, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            var owners = await _ownerCollection.Find(filter).Project<OwnerPartial>(projection).ToListAsync();
            return owners;
        }

        public async Task CreateAsync(Owner newOwner) =>
            await _ownerCollection.InsertOneAsync(newOwner);

        public async Task UpdateAsync(string id, Owner updatedOwner) =>
            await _ownerCollection.ReplaceOneAsync(x => x.OwnerId == id, updatedOwner);
        public async Task UpdateAsyncByFilterUpdateDefinitions(FilterDefinition<Owner>? filter, UpdateDefinition<Owner>? update)
        {
            await _ownerCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveAsync(string id) =>
            await _ownerCollection.DeleteOneAsync(x => x.OwnerId == id);
    }
}