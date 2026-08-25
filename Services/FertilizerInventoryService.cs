using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FertilizerInventoryService
    {
        private readonly IMongoCollection<FertilizerInventory> _fertilizerInventoryCollection;
        private readonly IMongoCollection<InputCatalog> _inputCatalogCollection;

        public FertilizerInventoryService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _fertilizerInventoryCollection = db.FarmGrow.GetCollection<FertilizerInventory>(settings.Value.FertilizerInventoryCollectionName);
            _inputCatalogCollection = db.FarmGrow.GetCollection<InputCatalog>(settings.Value.InputCatalogCollectionName);
        }

        public async Task<List<FertilizerInventory>> GetAsync() =>
            await _fertilizerInventoryCollection.Find(_ => true).ToListAsync();

        public async Task<List<FertilizerInventory>> GetAllFarmInventoryAsync(string farmId) =>
            await _fertilizerInventoryCollection.Find(i => i.FarmId == farmId).ToListAsync();

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
                    .Set(c => c.SuppliedDate, updatedFertilizerInventory.SuppliedDate)
                    .Set(c => c.UpdatedBy, updatedFertilizerInventory.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string inventoryId) =>
            await _fertilizerInventoryCollection.DeleteOneAsync(x => x.InventoryId == inventoryId);

        public async Task<List<InputCatalogNameDto>> GetInputCatalogNamesAsync(string type)
        {
            var catalogs = await _inputCatalogCollection
                .Find(x => x.Type == type && x.IsActive)
                .ToListAsync();

            return catalogs.Select(x => new InputCatalogNameDto
            {
                Name = x.Name,
                UnitType = x.UnitType,
                QuantityPerUnit = x.QuantityPerUnit
            }).ToList();
        }

        public async Task<InputCatalog?> GetInputCatalogByNameAndTypeAsync(string name, string type)
        {
            var trimmedName = name?.Trim() ?? "";
            var trimmedType = type?.Trim() ?? "";
            return await _inputCatalogCollection.Find(x =>
                x.Name.ToLower() == trimmedName.ToLower() &&
                x.Type.ToLower() == trimmedType.ToLower()
            ).FirstOrDefaultAsync();
        }

        public async Task CreateInputCatalogAsync(InputCatalog newCatalog) =>
            await _inputCatalogCollection.InsertOneAsync(newCatalog);

        public async Task ActivateInputCatalogAsync(string name, string type)
        {
            var trimmedName = name?.Trim() ?? "";
            var trimmedType = type?.Trim() ?? "";
            await _inputCatalogCollection.UpdateOneAsync(
                x => x.Name.ToLower() == trimmedName.ToLower() && x.Type.ToLower() == trimmedType.ToLower(),
                Builders<InputCatalog>.Update.Set(x => x.IsActive, true).Set(x => x.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveInputCatalogAsync(string name, string type)
        {
            var trimmedName = name?.Trim() ?? "";
            var trimmedType = type?.Trim() ?? "";
            await _inputCatalogCollection.DeleteOneAsync(
                x => x.Name.ToLower() == trimmedName.ToLower() && x.Type.ToLower() == trimmedType.ToLower()
            );
        }

        public async Task UpdateInputCatalogAsync(string oldName, string newName, string type, string? unitType, double? quantityPerUnit)
        {
            var trimmedOldName = oldName?.Trim() ?? "";
            var trimmedNewName = newName?.Trim() ?? "";
            var trimmedType = type?.Trim() ?? "";
            await _inputCatalogCollection.UpdateOneAsync(
                x => x.Name.ToLower() == trimmedOldName.ToLower() && x.Type.ToLower() == trimmedType.ToLower() && x.IsActive,
                Builders<InputCatalog>.Update
                    .Set(x => x.Name, trimmedNewName)
                    .Set(x => x.UnitType, unitType)
                    .Set(x => x.QuantityPerUnit, quantityPerUnit)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow)
            );
        }
    }
}
