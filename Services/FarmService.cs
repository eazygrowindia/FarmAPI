using FarmAPI.Models;
using FarmAPI.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class FarmService
    {
        private readonly IMongoCollection<Farm> _farmCollection;
        private readonly IMongoCollection<Crop> _cropCollection;
        private readonly IMongoCollection<Owner> _ownerCollection;
        private readonly IMongoCollection<Maintainer> _maintainerCollection;

        public FarmService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _farmCollection = db.FarmGrow.GetCollection<Farm>(settings.Value.FarmCollectionName);
            _cropCollection = db.FarmGrow.GetCollection<Crop>(settings.Value.CropCollectionName);
            _ownerCollection = db.FarmGrow.GetCollection<Owner>(settings.Value.OwnerCollectionName);
            _maintainerCollection = db.FarmGrow.GetCollection<Maintainer>(settings.Value.MaintainerCollectionName);
        }

        public async Task<List<Farm>> GetAsync() =>
            await _farmCollection.Find(_ => true).ToListAsync();

        public async Task<Farm?> GetAsync(string id) =>
            await _farmCollection.Find(x => x.FarmId == id).FirstOrDefaultAsync();

        public async Task<long> GetDistinctFarmsCountAsync()
        {
            var pipeline = _farmCollection.Aggregate()
                .Group(
                    key => key.FarmId,
                    g => new { Id = g.Key }           // group by CropId
                )
                .Count();                             // count distinct groups

            var result = await pipeline.FirstOrDefaultAsync();
            return result?.Count ?? 0;
        }

        public async Task<List<FarmPartial>> GetPartialFarmByIdOrNameOnUser(string userId, List<string> roles, string searchTerm)
        {
            List<FarmPartial> farmCollection = new List<FarmPartial>();
            var projection = Builders<Farm>.Projection.Include(f => f.FarmId)
                                           .Include(f => f.FarmName)
                                           //.Include(f => f.Crops)
                                           .Exclude("_id");
            var searchfilter = Builders<Farm>.Filter.Or(
                Builders<Farm>.Filter.Regex(x => x.FarmId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Farm>.Filter.Regex(x => x.FarmName, new BsonRegularExpression(searchTerm, "i"))
            );

            var searchfilterCrop = Builders<Crop>.Filter.Or(
                Builders<Crop>.Filter.Regex(x => x.CropId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Crop>.Filter.Regex(x => x.CropName, new BsonRegularExpression(searchTerm, "i"))
            );

            if (UserRolesHelper.HasRole(roles, UserRoles.FARMOWNER))
            {
                //get all farms from owner collection
                List<string> farms = new List<string>();
                var owner = await _ownerCollection.Find(o => o.UserId == userId).FirstOrDefaultAsync();
                if (owner != null)
                {
                    //farms = owner.FarmsOwned;

                    //var farmIdfilter = Builders<Farm>.Filter.In(f => f.FarmId, farms);
                    //var filter = farmIdfilter & searchfilter;
                    //farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();

                    var filter = Builders<Farm>.Filter.Eq(f => f.FarmOwnerId, owner.OwnerId) & searchfilter;
                    farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
                }
            }

            if (UserRolesHelper.HasRole(roles, UserRoles.FARMHELP))
            {
                //get all farms from maintainer collection
                List<string> farms = new List<string>();
                var maintainer = await _maintainerCollection.Find(o => o.UserId == userId).FirstOrDefaultAsync();
                if (maintainer != null)
                {
                    //farms = owner.FarmsMaintained;

                    //var farmIdfilter = Builders<Farm>.Filter.In(f => f.FarmId, farms);
                    //var filter = farmIdfilter & searchfilter;
                    //farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();

                    var filter = Builders<Farm>.Filter.Eq(f => f.FarmMaintainerId, maintainer.MaintainerId) & searchfilter;
                    farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
                }
            }

            if (UserRolesHelper.HasRole(roles, UserRoles.EASYGROWADMIN))
            {
                //get all farms from farm collection
                var farmIdfilter = Builders<Farm>.Filter.Empty;
                var filter = farmIdfilter & searchfilter;
                farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
            }


            var cropProjection = Builders<Crop>.Projection.Include(f => f.CropId)
                                           .Include(f => f.CropName)
                                           .Exclude("_id");

            foreach (var farm in farmCollection)
            {
                var cropDetails = new List<CropPartial>();
                //if (farm.Crops != null && farm.Crops.Count > 0)
                //{
                //    foreach (var cropId in farm.Crops)
                //    {
                //        var crop = await _cropCollection.Find(c => c.CropId == cropId).Project<CropPartial>(cropProjection).FirstOrDefaultAsync();
                //        if (crop != null)
                //        {
                //            cropDetails.Add(crop);
                //        }
                //    }
                //}
                var filterCrops = Builders<Crop>.Filter.Eq(c => c.FarmId, farm.FarmId) | searchfilterCrop;
                var crops = await _cropCollection.Find(filterCrops).Project<CropPartial>(cropProjection).ToListAsync();
                cropDetails.AddRange(crops);

                farm.CropDetail = cropDetails;
            }

            return farmCollection;
        }

        public async Task<List<FarmPartial>> GetPartialFarmByUser(string userId, List<string> roles)
        {
            List<FarmPartial> farmCollection = new List<FarmPartial>();
            var projection = Builders<Farm>.Projection.Include(f => f.FarmId)
                                           .Include(f => f.FarmName)
                                           //.Include(f => f.Crops)
                                           .Exclude("_id");

            if (UserRolesHelper.HasRole(roles, UserRoles.FARMOWNER))
            {
                //get all farms from owner collection
                List<string> farms = new List<string>();
                var owner = await _ownerCollection.Find(o => o.UserId == userId).FirstOrDefaultAsync();
                if (owner != null)
                {
                    //    farms = owner.FarmsOwned;
                    //}
                    //var filter = Builders<Farm>.Filter.In(f => f.FarmId, farms);
                    //farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();

                    var filter = Builders<Farm>.Filter.Eq(f => f.FarmOwnerId, owner.OwnerId);
                    farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
                }
            }

            if (UserRolesHelper.HasRole(roles, UserRoles.FARMHELP))
            {
                //get all farms from maintainer collection
                List<string> farms = new List<string>();
                var maintainer = await _maintainerCollection.Find(o => o.UserId == userId).FirstOrDefaultAsync();
                if (maintainer != null)
                {
                    //    farms = owner.FarmsMaintained;
                    //}
                    //var filter = Builders<Farm>.Filter.In(f => f.FarmId, farms);
                    //farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();

                    var filter = Builders<Farm>.Filter.Eq(f => f.FarmMaintainerId, maintainer.MaintainerId);
                    farmCollection = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
                }
            }

            if (UserRolesHelper.HasRole(roles, UserRoles.EASYGROWADMIN))
            {
                //get all farms from farm collection
                farmCollection = await _farmCollection.Find(_ => true).Project<FarmPartial>(projection).ToListAsync();
            }


            var cropProjection = Builders<Crop>.Projection.Include(f => f.CropId)
                                           .Include(f => f.CropName)
                                           .Exclude("_id");

            foreach (var farm in farmCollection)
            {
                var cropDetails = new List<CropPartial>();
                //if (farm.Crops != null && farm.Crops.Count > 0)
                //{
                //    foreach (var cropId in farm.Crops)
                //    {
                //        var crop = await _cropCollection.Find(c => c.CropId == cropId).Project<CropPartial>(cropProjection).FirstOrDefaultAsync();
                //        if (crop != null)
                //        {
                //            cropDetails.Add(crop);
                //        }
                //    }
                //}

                var filterCrops = Builders<Crop>.Filter.Eq(c => c.FarmId, farm.FarmId);
                var crops = await _cropCollection.Find(filterCrops).Project<CropPartial>(cropProjection).ToListAsync();
                cropDetails.AddRange(crops);

                farm.CropDetail = cropDetails;
            }

            return farmCollection;
        }

        public async Task<List<Farm>> GetFarmByIdOrName(string searchTerm)
        {
            var filter = Builders<Farm>.Filter.Or(
                Builders<Farm>.Filter.Regex(x => x.FarmId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Farm>.Filter.Regex(x => x.FarmName, new BsonRegularExpression(searchTerm, "i"))
            );

            return await _farmCollection.Find(filter).ToListAsync();
        }

        public async Task<List<FarmPartial>> SearchFarmsAsync(string searchTerm)
        {
            var projection = Builders<Farm>.Projection
                .Include(f => f.FarmId)
                .Include(f => f.FarmName)
                .Include(f => f.ShadeNetArea)
                .Exclude("_id");

            var filter = Builders<Farm>.Filter.Or(
                Builders<Farm>.Filter.Regex(x => x.FarmId, new BsonRegularExpression(searchTerm, "i")),
                Builders<Farm>.Filter.Regex(x => x.FarmName, new BsonRegularExpression(searchTerm, "i"))
            );

            var farms = await _farmCollection.Find(filter).Project<FarmPartial>(projection).ToListAsync();
            return farms;
        }

        public async Task CreateAsync(Farm newFarm) => 
            await _farmCollection.InsertOneAsync(newFarm);

        public async Task UpdateAsync(string id, Farm updatedFarm)
        {
            //await _farmCollection.ReplaceOneAsync(x => x.FarmId == id, updatedFarm);
            var filter = Builders<Farm>.Filter.Eq(x => x.FarmId, id);

            await _farmCollection.UpdateOneAsync(
                filter,
                Builders<Farm>.Update
                    .Set(f => f.FarmName, updatedFarm.FarmName)
                    .Set(f => f.Address, updatedFarm.Address)
                    .Set(f => f.ShadeNetArea, updatedFarm.ShadeNetArea)
                    //.Set(f => f.GeoTag, updatedFarm.GeoTag)
                    //.Set(f => f.WeatherData, updatedFarm.WeatherData)
                    .Set(f => f.FarmPondVolume, updatedFarm.FarmPondVolume)
                    .Set(f => f.IsSolarPowerAvailable, updatedFarm.IsSolarPowerAvailable)
                    .Set(f => f.MotorCapacity, updatedFarm.MotorCapacity)
                    .Set(f => f.AdditionalWaterSource, updatedFarm.AdditionalWaterSource)
                    .Set(f => f.WaterTestCertificateUrl, updatedFarm.WaterTestCertificateUrl)
                    .Set(f => f.IsSinglePhasePower, updatedFarm.IsSinglePhasePower)
                    .Set(f => f.IsThreePhasePower, updatedFarm.IsThreePhasePower)
                    .Set(f => f.GridPowerUnAvailability, updatedFarm.GridPowerUnAvailability)
                    .Set(f => f.AutomationRoomSize, updatedFarm.AutomationRoomSize)
                    .Set(f => f.FarmhouseNote, updatedFarm.FarmhouseNote)
                    .Set(f => f.StorageAreaNote, updatedFarm.StorageAreaNote)
                    //.Set(f => f.Crops, updatedFarm.Crops)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow)
            );
        }

        //public async Task FindAndUpdateAsync(string id, Farm updatedFarm)
        //{
        //    var filter = Builders<Farm>.Filter.Eq(x => x.FarmId, id);

        //    await _farmCollection.FindOneAndUpdateAsync(
        //        filter,
        //        Builders<Farm>.Update.Set(f => f.UpdatedAt, DateTime.UtcNow)
        //        );
        //}

        public async Task RemoveAsync(string id) =>
            await _farmCollection.DeleteOneAsync(x => x.FarmId == id);
    }
}