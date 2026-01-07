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
    public class KarnatakaImportService
    {
        private IMongoCollection<KarnatakaLocation> _collection;
        private readonly IMongoCollection<DistrictItem> _districtMappingcollection;
        private readonly string _karnatakaStateName;
        public KarnatakaImportService(IDatabaseFactory databaseFactory, IOptions<AddressLoaderDatabaseSettings> settings)
        {
            _collection = databaseFactory.AddressLoader.GetCollection<KarnatakaLocation>(settings.Value.KarnatakaLocationsCollectionName);
            _districtMappingcollection = databaseFactory.AddressLoader.GetCollection<DistrictItem>(settings.Value.KarnatakaBhoomiDistrictMappingCollectionName);
            _karnatakaStateName = settings.Value.KarnatakaStateName;
        }

        //public async Task<List<KarnatakaLocation>> GetHoblis(int district, int taluka)
        //{
        //    var filter = Builders<KarnatakaLocation>.Filter.Eq(x => x.DistrictCode, district) & Builders<KarnatakaLocation>.Filter.Eq(x => x.TalukaCode, taluka);
        //    return await _collection.Distinct(x => new { x.HobliCode, x.HobliName }, filter).ToListAsync();
        //}

        public async Task<long> HarvestKarnatakaHierarchyAsync(string outputCsvPath = null)
        {
            var projection = Builders<DistrictItem>.Projection.Include(f => f.DistrictCode)
                                           .Include(f => f.DistrictName)
                                           .Exclude("_id"); // Optional to exclude 

            //return await _cropCollection.Find(filter).Project<CropPartial>(projection).ToListAsync();

            var districtMappings = await _districtMappingcollection.Find(_ => true).Project<DistrictItem>(projection).ToListAsync();

            if (!districtMappings.Any())
                return 0;

            var districts = districtMappings
                .GroupBy(d => new { d.DistrictCode, d.DistrictName })
                .Select(g => new DistrictInfo
                {
                    DistrictCode = g.Key.DistrictCode,
                    DistrictName = g.Key.DistrictName
                    //DistrictName = $"District_{g.Key.DistrictCode}"
                })
                .OrderBy(d => d.DistrictName)
                .ToList();

            var allData = new List<KarnatakaLocation>();
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            // Step 1: Districts (static list, Districtcode=1 to 31)
            //var districts = Enumerable.Range(1, 31).Select(d => new { DistrictCode = d, DistrictName = $"District_{d}" }).ToList();  // Map names later
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Bonus: Ignore minor casing diffs
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  // Kannada Unicode safe
            };

            foreach (var dist in districts)
            {
                // Step 2: Talukas ?Districtcode={dist.DistrictCode}
                var talukaUrl = $"https://rdservices.karnataka.gov.in/Service84/Default/FillCensusTALUKA?Districtcode={dist.DistrictCode}";
                var talukaJson = await httpClient.GetStringAsync(talukaUrl);
                // talukaJson is currently: "\"[{\\\"TALUKA_CODE...}]\""

                string fullyEscapedJson = Regex.Unescape(talukaJson);
                // This variable now holds: "\[{\"TALUKA_CODE...}]"

                string cleanJsonString = fullyEscapedJson.Trim('"');
                // This variable now holds: "[{"TALUKA_CODE...}]"

                var talukas = JsonSerializer.Deserialize<List<TalukaItem>>(cleanJsonString, options) ?? new();  // Assume [{"id":1,"name":"Bangalore North"},...]

                foreach (var taluk in talukas)
                {
                    // Step 3: Hoblis ?Districtcode={dist}&talukaCode={taluk.id}
                    var hobliUrl = $"https://rdservices.karnataka.gov.in/Service84/Default/FillCensushobli?Districtcode={dist.DistrictCode}&talukaCode={taluk.TALUKA_CODE}";
                    var hobliJson = await httpClient.GetStringAsync(hobliUrl);
                    hobliJson = Regex.Unescape(hobliJson).Trim('"');
                    var hoblis = JsonSerializer.Deserialize<List<HobliItem>>(hobliJson) ?? new();

                    foreach (var hobli in hoblis)
                    {
                        // Step 4: Villages ?Districtcode={dist}&talukaCode={taluk.id}&hobliCode={hobli.id}
                        var villageUrl = $"https://rdservices.karnataka.gov.in/Service84/Default/FillCensusvillege?Districtcode={dist.DistrictCode}&talukaCode={taluk.TALUKA_CODE}&hobliCode={hobli.HOBLI_CODE}";
                        var villageJson = await httpClient.GetStringAsync(villageUrl);
                        villageJson = Regex.Unescape(villageJson).Trim('"');
                        var villages = JsonSerializer.Deserialize<List<VillageItem>>(villageJson) ?? new();

                        // Flatten: One doc per village
                        allData.AddRange(villages.Select(v => new KarnatakaLocation
                        {
                            StateCode = 0,
                            StateName = _karnatakaStateName,
                            DistrictCode = dist.DistrictCode,
                            DistrictName = dist.DistrictName,  // Resolve via static map
                            TalukaCode = taluk.TALUKA_CODE,
                            TalukaName = taluk.TALUKA_NAME,
                            HobliCode = hobli.HOBLI_CODE,
                            HobliName = hobli.HOBLI_NAME,
                            VillageCode = v.VILLAGE_CODE,
                            VillageName = v.VILLAGE_NAME
                        }));
                    }
                }
            }

            // Bulk insert (your existing method)
            await _collection.InsertManyAsync(allData);

            // Optional: Export CSV for backup
            //if (!string.IsNullOrEmpty(outputCsvPath)) ExportToCsv(allData, outputCsvPath);

            return (long)allData.Count;
        }

    }
}
