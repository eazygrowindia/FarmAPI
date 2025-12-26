using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FertilizerInventoryService
    {
        private readonly IMongoCollection<FertilizerInventory> _fertilizerInventoryCollection;

        public FertilizerInventoryService(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _fertilizerInventoryCollection = db.GetCollection<FertilizerInventory>(settings.Value.FertilizerInventoryCollectionName);
        }

        public async Task<List<FertilizerInventory>> GetAsync() =>
            await _fertilizerInventoryCollection.Find(_ => true).ToListAsync();

        public async Task<FertilizerInventory?> GetByIdAsync(string id) =>
            await _fertilizerInventoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<FertilizerInventory?> GetByInventoryIdAsync(string inventoryId) =>
            await _fertilizerInventoryCollection.Find(x => x.InventoryId == inventoryId).FirstOrDefaultAsync();

        public async Task CreateAsync(FertilizerInventory newFertilizerInventory) => 
            await _fertilizerInventoryCollection.InsertOneAsync(newFertilizerInventory);

        public async Task UpdateAsync(FertilizerInventory updatedFertilizerInventory)
        {
            var filter = Builders<FertilizerInventory>.Filter.And(
                Builders<FertilizerInventory>.Filter.Eq(c => c.InventoryId, updatedFertilizerInventory.InventoryId)
                );

            await _fertilizerInventoryCollection.UpdateOneAsync(filter,
                Builders<FertilizerInventory>.Update
                    .Set(c => c.FertilizerName, updatedFertilizerInventory.FertilizerName)
                    .Set(c => c.QuantitySupplied, updatedFertilizerInventory.QuantitySupplied)
                    .Set(c => c.SuppliedDate, updatedFertilizerInventory.SuppliedDate)
                    .Set(c => c.QuantityUsed, updatedFertilizerInventory.QuantityUsed)
                    .Set(c => c.UsedDate, updatedFertilizerInventory.UsedDate)
                    .Set(c => c.UpdatedBy, updatedFertilizerInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string inventoryId) =>
            await _fertilizerInventoryCollection.DeleteOneAsync(x => x.InventoryId == inventoryId);
    }
}