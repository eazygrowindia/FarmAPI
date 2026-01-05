using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class DiseaseControlInventoryService
    {
        private readonly IMongoCollection<DiseaseControlInventory> _diseaseControlInventoryCollection;

        public DiseaseControlInventoryService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _diseaseControlInventoryCollection = db.FarmGrow.GetCollection<DiseaseControlInventory>(settings.Value.DiseaseControlInventoryCollectionName);
        }

        public async Task<List<DiseaseControlInventory>> GetAsync() =>
            await _diseaseControlInventoryCollection.Find(_ => true).ToListAsync();

        public async Task<List<DiseaseControlInventory>> GetAllFarmInventoryAsync(string farmId) =>
            await _diseaseControlInventoryCollection.Find(i => i.FarmId == farmId).ToListAsync();

        public async Task<DiseaseControlInventory?> GetByIdAsync(string id) =>
            await _diseaseControlInventoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<DiseaseControlInventory?> GetByInventoryIdAsync(string inventoryId) =>
            await _diseaseControlInventoryCollection.Find(x => x.InventoryId == inventoryId).FirstOrDefaultAsync();

        public async Task<DiseaseControlInventory?> GetByFarmIdDiseaseControlNameAsync(string farmId, string diseaseControlName) =>
            await _diseaseControlInventoryCollection.Find(x => x.FarmId == farmId && x.DiseaseControlName == diseaseControlName).FirstOrDefaultAsync();

        public async Task CreateAsync(DiseaseControlInventory newFertilizerInventory) => 
            await _diseaseControlInventoryCollection.InsertOneAsync(newFertilizerInventory);

        public async Task UpdateAsync(DiseaseControlInventory updatedFertilizerInventory)
        {
            var filter = Builders<DiseaseControlInventory>.Filter.And(
                Builders<DiseaseControlInventory>.Filter.Eq(c => c.InventoryId, updatedFertilizerInventory.InventoryId)
                );

            await _diseaseControlInventoryCollection.UpdateOneAsync(filter,
                Builders<DiseaseControlInventory>.Update
                    .Set(c => c.DiseaseControlName, updatedFertilizerInventory.DiseaseControlName)
                    .Set(c => c.QuantitySupplied, updatedFertilizerInventory.QuantitySupplied)
                    .Set(c => c.SuppliedDate, updatedFertilizerInventory.SuppliedDate)
                    .Set(c => c.QuantityUsed, updatedFertilizerInventory.QuantityUsed)
                    .Set(c => c.UsedDate, updatedFertilizerInventory.UsedDate)
                    .Set(c => c.UpdatedBy, updatedFertilizerInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string inventoryId) =>
            await _diseaseControlInventoryCollection.DeleteOneAsync(x => x.InventoryId == inventoryId);
    }
}