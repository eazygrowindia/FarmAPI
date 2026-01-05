using CsvHelper;
using CsvHelper.Configuration;
using DnsClient.Internal;
using FarmAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

namespace FarmAPI.Services
{
    public class LgdImportService
    {
        private readonly IMongoCollection<LgdLocation> _collection;

        public LgdImportService(IDatabaseFactory dbFactory, IOptions<AddressLoaderDatabaseSettings> settings)
        {
            //_collection = database.GetCollection<LgdLocation>(settings.Value.LgdLocationsCollectionName);
            _collection = dbFactory.AddressLoader.GetCollection<LgdLocation>(settings.Value.LgdLocationsCollectionName);
        }

        public async Task<long> ImportStateCsvAsync(string csvPath, string processedFileName)
        {
            using var reader = new StreamReader(csvPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                PrepareHeaderForMatch = args => args.Header.ToLower().Trim(),
                MissingFieldFound = null
            });

            var records = new List<LgdLocation>();
            long totalInserted = 0;
            const int batchSize = 5000;  // Matches your original batching

            while (await csv.ReadAsync())
            {
                var row = csv.GetRecord<LgdLocationRow>()!;  // Your working row class

                var doc = new LgdLocation
                {
                    StateCode = row.stateCode,
                    StateName = row.stateNameEnglish ?? "",
                    DistrictCode = row.districtCode,
                    DistrictName = row.districtNameEnglish ?? "",
                    SubdistrictCode = row.subdistrictCode,
                    SubdistrictName = row.subdistrictNameEnglish ?? "",
                    VillageCode = row.villageCode,
                    VillageName = row.villageNameEnglish ?? "",
                    Pincode = row.pincode ?? "",
                    SourceStateFile = processedFileName,
                    LastUpdatedAt = DateTime.UtcNow
                };

                records.Add(doc);

                if (records.Count >= batchSize)
                {
                    totalInserted += await BulkInsertAsync(records);
                    records.Clear();
                }
            }

            if (records.Count > 0)
            {
                totalInserted += await BulkInsertAsync(records);
            }

            return totalInserted;
        }

        private async Task<int> BulkInsertAsync(List<LgdLocation> batch)
        {
            await _collection.InsertManyAsync(batch, new InsertManyOptions
            {
                IsOrdered = false,  // Parallel + continue on dupes
                BypassDocumentValidation = true
            });
            return batch.Count;
        }

        //public async Task<int> SyncCsvFromStreamAsync(Stream csvStream, string sourceFileName)
        //{
        //    if (csvStream == null || csvStream.Length == 0)
        //    {
        //        Console.WriteLine("Sync skipped: empty stream for {Source}", sourceFileName);
        //        return 0;
        //    }

        //    const int batchSize = 1000;
        //    var requests = new List<WriteModel<LgdLocation>>();
        //    var synced = 0;
        //    var sw = Stopwatch.StartNew();

        //    csvStream.Position = 0;

        //    try
        //    {
        //        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        //        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        //        {
        //            HasHeaderRecord = true,
        //            Delimiter = ",",
        //            PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),  // .NET 10 fix [web:55]
        //            //HeaderValidated = null,
        //            MissingFieldFound = null,
        //            //BadDataFound = null
        //        });

        //        await csv.ReadAsync();

        //        while (await csv.ReadAsync())
        //        {
        //            var row = csv.GetRecord<LgdLocationRow>();
        //            if (string.IsNullOrWhiteSpace(row?.pincode)) continue;  // Skip invalid rows

        //            var doc = MapToLgdLocation(row);  // Your mapping method
        //            doc.SourceStateFile = sourceFileName;
        //            doc.LastSynced = DateTime.UtcNow;

        //            // Unique filter for upsert
        //            var filter = Builders<LgdLocation>.Filter.And(
        //                Builders<LgdLocation>.Filter.Eq(x => x.Pincode, doc.Pincode),
        //                Builders<LgdLocation>.Filter.Eq(x => x.VillageCode, doc.VillageCode)
        //            );

        //            var update = Builders<LgdLocation>.Update
        //                .Set(x => x.StateCode, doc.StateCode)
        //                .Set(x => x.StateName, doc.StateName ?? "")
        //                .Set(x => x.DistrictCode, doc.DistrictCode)
        //                .Set(x => x.DistrictName, doc.DistrictName ?? "")
        //                .Set(x => x.SubdistrictCode, doc.SubdistrictCode)
        //                .Set(x => x.SubdistrictName, doc.SubdistrictName ?? "")
        //                .Set(x => x.VillageName, doc.VillageName ?? "")
        //                .Set(x => x.LastSynced, doc.LastSynced)
        //                .SetOnInsert(x => x.FirstImported, DateTime.UtcNow)
        //                .SetOnInsert(x => x.SourceStateFile, sourceFileName);

        //            requests.Add(new UpdateOneModel<LgdLocation>(filter, update) { IsUpsert = true });

        //            if (requests.Count >= batchSize)
        //            {
        //                await BulkSyncAsync(requests);
        //                synced += requests.Count;
        //                requests.Clear();
        //            }
        //        }

        //        if (requests.Count > 0)
        //        {
        //            await BulkSyncAsync(requests);
        //            synced += requests.Count;
        //        }

        //        sw.Stop();
        //        Console.WriteLine("Synced {Synced} docs from {Source} ({Rate:F0}/min)",
        //            synced, sourceFileName, synced / Math.Max(sw.Elapsed.TotalMinutes, 0.001));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Stream sync failed for {Source}", sourceFileName);
        //        throw;
        //    }

        //    return synced;
        //}

        //private async Task BulkSyncAsync(List<WriteModel<LgdLocation>> requests)
        //{
        //    await _collection.BulkWriteAsync(requests, new BulkWriteOptions
        //    {
        //        IsOrdered = false,
        //        BypassDocumentValidation = true
        //    });
        //}

        //private LgdLocation MapToLgdLocation(LgdLocationRow row)
        //{
        //    return new LgdLocation
        //    {
        //        Pincode = row.pincode,
        //        StateCode = row.stateCode,
        //        StateName = row.stateNameEnglish?.Trim(),
        //        DistrictCode = row.districtCode,
        //        DistrictName = row.districtNameEnglish?.Trim(),
        //        SubdistrictCode = row.subdistrictCode,
        //        SubdistrictName = row.subdistrictNameEnglish?.Trim(),
        //        VillageCode = row.villageCode,
        //        VillageName = row.villageNameEnglish?.Trim()
        //    };
        //}


    }
}
