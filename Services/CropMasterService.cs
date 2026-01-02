using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class CropMasterService
    {
        private readonly IMongoCollection<CropMaster> _cropMasterCollection;

        public CropMasterService(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _cropMasterCollection = db.GetCollection<CropMaster>(settings.Value.CropMasterCollectionName);
        }

        public async Task<List<CropMaster>> GetAsync() =>
            await _cropMasterCollection.Find(_ => true).ToListAsync();

        public async Task<CropMaster?> GetByIdAsync(string id) =>
            await _cropMasterCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<CropMaster?> GetByCropIdAsync(string cropId) =>
            await _cropMasterCollection.Find(x => x.CropId == cropId).FirstOrDefaultAsync();

        public async Task<CropMaster?> GetByCropNameAsync(string cropName) =>
            await _cropMasterCollection.Find(x => x.CropName == cropName).FirstOrDefaultAsync();

        public async Task CreateAsync(CropMaster newCropMaster) => 
            await _cropMasterCollection.InsertOneAsync(newCropMaster);

        public async Task UpdateAsync(CropMaster updatedCropMaster)
        {
            var filter = Builders<CropMaster>.Filter.And(
                Builders<CropMaster>.Filter.Eq(c => c.CropId, updatedCropMaster.CropId)
                );

            await _cropMasterCollection.UpdateOneAsync(filter,
                Builders<CropMaster>.Update
                    .Set(c => c.CropName, updatedCropMaster.CropName)
                    .Set(c => c.Duration, updatedCropMaster.Duration)
                    .Set(c => c.ExpectedYield, updatedCropMaster.ExpectedYield)
                    .Set(c => c.SowingTime, updatedCropMaster.SowingTime)
                    .Set(c => c.HarvestTime, updatedCropMaster.HarvestTime)
                    .Set(c => c.SowingMethod, updatedCropMaster.SowingMethod)
                    .Set(c => c.PestsAndDiseases, updatedCropMaster.PestsAndDiseases)
                    .Set(c => c.MoleculesToAdd, updatedCropMaster.MoleculesToAdd)
                    .Set(c => c.UpdatedBy, updatedCropMaster.UpdatedBy)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow)
            );
        }

        public async Task RemoveAsync(string cropId) =>
            await _cropMasterCollection.DeleteOneAsync(x => x.CropId == cropId);
    }
}