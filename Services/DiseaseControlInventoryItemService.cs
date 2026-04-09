using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class DiseaseControlInventoryItemService
    {
        private readonly IMongoCollection<DiseaseControlInventoryItem> _diseaseControlInventoryItemCollection;

        public DiseaseControlInventoryItemService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _diseaseControlInventoryItemCollection = db.FarmGrow.GetCollection<DiseaseControlInventoryItem>(settings.Value.DiseaseControlInventoryItemCollectionName);
        }

        public async Task<List<DiseaseControlInventoryItem>> GetAsync() =>
            await _diseaseControlInventoryItemCollection.Find(_ => true).ToListAsync();

        public async Task<DiseaseControlInventoryItem?> GetByIdAsync(string id) =>
            await _diseaseControlInventoryItemCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<List<DiseaseControlInventoryItem>> GetByInventoryIdAsync(string inventoryId) =>
            await _diseaseControlInventoryItemCollection.Find(x => x.InventoryId == inventoryId).ToListAsync();

        public async Task<List<DiseaseControlInventoryItem>> GetByMultipleInventoryIdDiseaseControlNameAsync(List<string> inventoryIds, string diseaseControlName)
        {
            var filterBuilder = Builders<DiseaseControlInventoryItem>.Filter;

            var filter = filterBuilder.And(
                filterBuilder.In(x => x.InventoryId, inventoryIds),
                // Use Regex for case-insensitive match
                filterBuilder.Regex(x => x.DiseaseControlName, new BsonRegularExpression($"^{diseaseControlName}$", "i")),
                // Compare fields in the same document
                filterBuilder.Where(x => x.QuantitySupplied > x.QuantityUsed)
            );

            return await _diseaseControlInventoryItemCollection.Find(filter).ToListAsync();
        }

        public async Task<DiseaseControlInventoryItem?> GetByInventoryItemIdAsync(string inventoryItemId) =>
            await _diseaseControlInventoryItemCollection.Find(x => x.InventoryItemId == inventoryItemId).FirstOrDefaultAsync();

        public async Task<DiseaseControlInventoryItem?> GetByinventoryItemIdDiseaseControlNameAsync(string inventoryItemId, string diseaseControlName) =>
            await _diseaseControlInventoryItemCollection.Find(x => x.InventoryItemId == inventoryItemId && x.DiseaseControlName == diseaseControlName).FirstOrDefaultAsync();

        public async Task CreateOneAsync(DiseaseControlInventoryItem newDiseaseControlInventoryItem) => 
            await _diseaseControlInventoryItemCollection.InsertOneAsync(newDiseaseControlInventoryItem);

        public async Task CreateManyAsync(List<DiseaseControlInventoryItem> newDiseaseControlInventoryItems) =>
            await _diseaseControlInventoryItemCollection.InsertManyAsync(newDiseaseControlInventoryItems);

        public async Task ConsumeQuantityAsync(string inventoryItemId, DiseaseControlInventoryItem updatedDiseaseControlInventory)
        {
            var filter = Builders<DiseaseControlInventoryItem>.Filter.And(
                Builders<DiseaseControlInventoryItem>.Filter.Eq(c => c.InventoryItemId, inventoryItemId)
                );

            await _diseaseControlInventoryItemCollection.UpdateOneAsync(filter,
                Builders<DiseaseControlInventoryItem>.Update
                    .Set(c => c.QuantityUsed, updatedDiseaseControlInventory.QuantityUsed)
                    .Set(c => c.UsedDate, DateTime.UtcNow)
                    .Set(c => c.UpdatedBy, updatedDiseaseControlInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task UpdateAsync(DiseaseControlInventoryItem updatedDiseaseControlInventory)
        {
            var filter = Builders<DiseaseControlInventoryItem>.Filter.And(
                Builders<DiseaseControlInventoryItem>.Filter.Eq(c => c.InventoryId, updatedDiseaseControlInventory.InventoryId)
                );

            await _diseaseControlInventoryItemCollection.UpdateOneAsync(filter,
                Builders<DiseaseControlInventoryItem>.Update
                    .Set(c => c.DiseaseControlName, updatedDiseaseControlInventory.DiseaseControlName)
                    .Set(c => c.QuantitySupplied, updatedDiseaseControlInventory.QuantitySupplied)
                    .Set(c => c.QuantityUsed, updatedDiseaseControlInventory.QuantityUsed)
                    .Set(c => c.UsedDate, updatedDiseaseControlInventory.UsedDate)
                    .Set(c => c.UpdatedBy, updatedDiseaseControlInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string inventoryId) =>
            await _diseaseControlInventoryItemCollection.DeleteManyAsync(x => x.InventoryId == inventoryId);
    }
}
