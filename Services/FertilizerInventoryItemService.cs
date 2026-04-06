using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FertilizerInventoryItemService
    {
        private readonly IMongoCollection<FertilizerInventoryItem> _fertilizerInventoryItemCollection;

        public FertilizerInventoryItemService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _fertilizerInventoryItemCollection = db.FarmGrow.GetCollection<FertilizerInventoryItem>(settings.Value.FertilizerInventoryItemCollectionName);
        }

        public async Task<List<FertilizerInventoryItem>> GetAsync() =>
            await _fertilizerInventoryItemCollection.Find(_ => true).ToListAsync();

        public async Task<FertilizerInventoryItem?> GetByIdAsync(string id) =>
            await _fertilizerInventoryItemCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<List<FertilizerInventoryItem>> GetByInventoryIdAsync(string inventoryId) =>
            await _fertilizerInventoryItemCollection.Find(x => x.InventoryId == inventoryId).ToListAsync();

        public async Task<List<FertilizerInventoryItem>> GetByMultipleInventoryIdFertilizerNameAsync(List<string> inventoryIds, string fertilizerName)
        {
            //await _fertilizerInventoryItemCollection.Find(x => inventoryIds.Contains(x.InventoryId) && x.FertilizerName == fertilizerName).ToListAsync();

            var filterBuilder = Builders<FertilizerInventoryItem>.Filter;

            var filter = filterBuilder.And(
                filterBuilder.In(x => x.InventoryId, inventoryIds),
                // Use Regex for case-insensitive match
                filterBuilder.Regex(x => x.FertilizerName, new BsonRegularExpression($"^{fertilizerName}$", "i")),
                // Compare fields in the same document
                filterBuilder.Where(x => x.QuantitySupplied > x.QuantityUsed)
            );

            return await _fertilizerInventoryItemCollection.Find(filter).ToListAsync();
        }

        public async Task<FertilizerInventoryItem?> GetByInventoryItemIdAsync(string inventoryItemId) =>
            await _fertilizerInventoryItemCollection.Find(x => x.InventoryItemId == inventoryItemId).FirstOrDefaultAsync();

        public async Task<FertilizerInventoryItem?> GetByinventoryItemIdFertilizerNameAsync(string inventoryItemId, string fertilizerName) =>
            await _fertilizerInventoryItemCollection.Find(x => x.InventoryItemId == inventoryItemId && x.FertilizerName == fertilizerName).FirstOrDefaultAsync();

        public async Task CreateOneAsync(FertilizerInventoryItem newFertilizerInventoryItem) => 
            await _fertilizerInventoryItemCollection.InsertOneAsync(newFertilizerInventoryItem);

        public async Task CreateManyAsync(List<FertilizerInventoryItem> newFertilizerInventoryItems) =>
            await _fertilizerInventoryItemCollection.InsertManyAsync(newFertilizerInventoryItems);

        public async Task ConsumeQuantityAsync(string inventoryItemId,FertilizerInventoryItem updatedFertilizerInventory)
        {
            var filter = Builders<FertilizerInventoryItem>.Filter.And(
                Builders<FertilizerInventoryItem>.Filter.Eq(c => c.InventoryItemId, inventoryItemId)
                );

            await _fertilizerInventoryItemCollection.UpdateOneAsync(filter,
                Builders<FertilizerInventoryItem>.Update
                    .Set(c => c.QuantityUsed, updatedFertilizerInventory.QuantityUsed)
                    .Set(c => c.UsedDate, DateTime.UtcNow)
                    .Set(c => c.UpdatedBy, updatedFertilizerInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task UpdateAsync(FertilizerInventoryItem updatedFertilizerInventory)
        {
            var filter = Builders<FertilizerInventoryItem>.Filter.And(
                Builders<FertilizerInventoryItem>.Filter.Eq(c => c.InventoryId, updatedFertilizerInventory.InventoryId)
                );

            await _fertilizerInventoryItemCollection.UpdateOneAsync(filter,
                Builders<FertilizerInventoryItem>.Update
                    .Set(c => c.FertilizerName, updatedFertilizerInventory.FertilizerName)
                    .Set(c => c.QuantitySupplied, updatedFertilizerInventory.QuantitySupplied)
                    .Set(c => c.QuantityUsed, updatedFertilizerInventory.QuantityUsed)
                    .Set(c => c.UsedDate, updatedFertilizerInventory.UsedDate)
                    .Set(c => c.UpdatedBy, updatedFertilizerInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string inventoryId) =>
            await _fertilizerInventoryItemCollection.DeleteManyAsync(x => x.InventoryId == inventoryId);
    }
}