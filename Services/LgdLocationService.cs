using CsvHelper;
using CsvHelper.Configuration;
using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;

namespace FarmAPI.Services
{
    public class LgdLocationService
    {
        private readonly IMongoCollection<LgdLocation> _collection;

        public LgdLocationService(IDatabaseFactory database, IOptions<AddressLoaderDatabaseSettings> settings)
        {
            _collection = database.AddressLoader.GetCollection<LgdLocation>(settings.Value.LgdLocationsCollectionName);
            //CreateIndexes();
        }

        /// <summary>
        /// Not using this since there are lot of duplicates in the data dump
        /// </summary>
        private void CreateIndexes()
        {
            var indexKeys1 = Builders<LgdLocation>.IndexKeys.Ascending(x => x.Pincode);
            var indexOptions1 = new CreateIndexModel<LgdLocation>(indexKeys1);

            var indexKeys2 = Builders<LgdLocation>.IndexKeys
                .Ascending(x => x.Pincode)
                .Ascending(x => x.DistrictCode)
                .Ascending(x => x.SubdistrictCode);
            var indexOptions2 = new CreateIndexModel<LgdLocation>(indexKeys2);

            var indexKeys3 = Builders<LgdLocation>.IndexKeys
                .Ascending(x => x.StateCode)
                .Ascending(x => x.DistrictCode)
                .Ascending(x => x.SubdistrictCode);
            var indexOptions3 = new CreateIndexModel<LgdLocation>(indexKeys3);

            var indexKeys4 = Builders<LgdLocation>.IndexKeys.Ascending(x => x.VillageCode);
            var indexOptions4 = new CreateIndexModel<LgdLocation>(indexKeys4, new CreateIndexOptions
            {
                Unique = true
            });

            _collection.Indexes.CreateMany(new[] { indexOptions1, indexOptions2, indexOptions3, indexOptions4 });
        }

        public async Task<PincodeResponse> GetByPincodeAsync(string pincode)
        {
            var filter = Builders<LgdLocation>.Filter.Eq(x => x.Pincode, pincode);
            var docs = await _collection.Find(filter).ToListAsync();

            if (!docs.Any())
                return new PincodeResponse();

            var state = docs.GroupBy(d => new { d.StateCode, d.StateName })
                        .Select(g => new StateInfo
                        {
                            StateCode = g.Key.StateCode,
                            StateName = g.Key.StateName
                        })
                        .Single();

            var districts = docs
                .GroupBy(d => new { d.DistrictCode, d.DistrictName })
                .Select(g => new DistrictInfo
                {
                    DistrictCode = g.Key.DistrictCode,
                    DistrictName = g.Key.DistrictName
                })
                .OrderBy(d => d.DistrictName)
                .ToList();

            var subdistricts = docs
                .GroupBy(d => new { d.SubdistrictCode, d.SubdistrictName })
                .Select(g => new SubdistrictInfo
                {
                    SubdistrictCode = g.Key.SubdistrictCode,
                    SubdistrictName = g.Key.SubdistrictName
                })
                .OrderBy(s => s.SubdistrictName)
                .ToList();

            var villages = docs
                .GroupBy(d => new { d.VillageCode, d.VillageName })
                .Select(g => new VillageInfo
                {
                    VillageCode = g.Key.VillageCode,
                    VillageName = g.Key.VillageName
                })
                .OrderBy(v => v.VillageName)
                .ToList();

            return new PincodeResponse
            {
                State = state,
                Districts = districts,
                Subdistricts = subdistricts,
                Villages = villages
            };
        }

        public async Task<List<DistrictInfo>> GetDistrictsAsync(string pincode)
        {
            var docs = await _collection.Find(x => x.Pincode == pincode).ToListAsync();

            var districts = docs
                .GroupBy(d => new { d.DistrictCode, d.DistrictName })
                .Select(g => new DistrictInfo
                {
                    DistrictCode = g.Key.DistrictCode,
                    DistrictName = g.Key.DistrictName
                })
                .OrderBy(d => d.DistrictName)
                .ToList();
            return districts;
        }

        public async Task<List<SubdistrictInfo>> GetSubdistrictsAsync(string pincode, int districtCode)
        {
            var filter = Builders<LgdLocation>.Filter.And(
                Builders<LgdLocation>.Filter.Eq(x => x.Pincode, pincode),
                Builders<LgdLocation>.Filter.Eq(x => x.DistrictCode, districtCode)
            );

            var docs = await _collection.Find(filter).ToListAsync();

            var subdistricts = docs
                .GroupBy(d => new { d.SubdistrictCode, d.SubdistrictName })
                .Select(g => new SubdistrictInfo
                {
                    SubdistrictCode = g.Key.SubdistrictCode,
                    SubdistrictName = g.Key.SubdistrictName
                })
                .OrderBy(s => s.SubdistrictName)
                .ToList();

            return subdistricts;
        }

        public async Task<List<VillageInfo>> GetVillagesAsync(string pincode, int districtCode, int subdistrictCode)
        {
            var filter = Builders<LgdLocation>.Filter.And(
                Builders<LgdLocation>.Filter.Eq(x => x.Pincode, pincode),
                Builders<LgdLocation>.Filter.Eq(x => x.DistrictCode, districtCode),
                Builders<LgdLocation>.Filter.Eq(x => x.SubdistrictCode, subdistrictCode)
            );

            var docs = await _collection.Find(filter).ToListAsync();

            var villages = docs
                .GroupBy(d => new { d.VillageCode, d.VillageName })
                .Select(g => new VillageInfo
                {
                    VillageCode = g.Key.VillageCode,
                    VillageName = g.Key.VillageName
                })
                .OrderBy(v => v.VillageName)
                .ToList();

            return villages;
        }
    }
}
