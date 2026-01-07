using FarmAPI.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FarmAPI.Services
{
    public class KarnatakaLocationService
    {
        private IMongoCollection<KarnatakaLocation> _collection;
        private readonly string _karnatakaStateName;

        public KarnatakaLocationService(IDatabaseFactory databaseFactory, IOptions<AddressLoaderDatabaseSettings> settings)
        {
            _collection = databaseFactory.AddressLoader.GetCollection<KarnatakaLocation>(settings.Value.KarnatakaLocationsCollectionName);
            _karnatakaStateName = settings.Value.KarnatakaStateName;
        }

        public async Task<List<DistrictInfo>> GetDistrictsAsync()
        {
            var docs = await _collection.Find(_ => true).ToListAsync();

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

        public async Task<List<TalukaResponse>> GetTalukasByDistrictAsync(int districtCode)
        {
            var docs = await _collection.Find(x => x.DistrictCode == districtCode).ToListAsync();

            var subdistricts = docs
                .GroupBy(d => new { d.TalukaCode, d.TalukaName })
                .Select(g => new TalukaResponse
                {
                    TalukaCode = g.Key.TalukaCode,
                    TalukaName = g.Key.TalukaName
                })
                .OrderBy(s => s.TalukaName)
                .ToList();

            return subdistricts;
        }

        public async Task<List<HobliResponse>> GetHoblisByDistrictAndTalukaAsync(int districtCode, int talukaCode)
        {
            var filter = Builders<KarnatakaLocation>.Filter.Eq(x => x.DistrictCode, districtCode) 
                & Builders<KarnatakaLocation>.Filter.Eq(x => x.TalukaCode, talukaCode);
            var docs = await _collection.Find(filter).ToListAsync();
            var hoblis = docs
                .GroupBy(d => new { d.HobliCode, d.HobliName })
                .Select(g => new HobliResponse
                {
                    HobliCode = g.Key.HobliCode,
                    HobliName = g.Key.HobliName
                })
                .OrderBy(h => h.HobliName)
                .ToList();

            return hoblis;
        }

        public async Task<List<VillageInfo>> GetVillagesByDistrictAndTalukaAndHobliAsync(int districtCode, int talukaCode, int hobliCode)
        {
            var filter = Builders<KarnatakaLocation>.Filter.Eq(x => x.DistrictCode, districtCode)
                & Builders<KarnatakaLocation>.Filter.Eq(x => x.TalukaCode, talukaCode)
                & Builders<KarnatakaLocation>.Filter.Eq(x => x.HobliCode, hobliCode);

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
