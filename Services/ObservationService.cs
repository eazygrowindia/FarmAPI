using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class ObservationService
    {
        private readonly IMongoCollection<Observation> _observationCollection;

        public ObservationService(IDatabaseFactory db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _observationCollection = db.FarmGrow.GetCollection<Observation>(settings.Value.ObservationCollectionName);
        }

        public async Task<List<Observation>> GetAsync() =>
            await _observationCollection.Find(_ => true).ToListAsync();

        public async Task<Observation?> GetByIdAsync(string id) =>
            await _observationCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Observation?> GetByObservationIdAsync(string observationId) =>
            await _observationCollection.Find(x => x.ObservationId == observationId).FirstOrDefaultAsync();

        public async Task<List<Observation>?> GetByCropIdAsync(string cropId) =>
            await _observationCollection.Find(x => x.CropId == cropId).ToListAsync();

        public async Task<long> GetTodayNewObservationCountAsync()
        {
            // Work in UTC; adjust if you need local time window
            var today = DateTime.UtcNow.Date;          // today 00:00 UTC
            var tomorrow = today.AddDays(1);           // next day 00:00 UTC

            var builder = Builders<Observation>.Filter;

            var filter = builder.Gte(a => a.CreatedAt, today) &
                         builder.Lt(a => a.CreatedAt, tomorrow);

            return await _observationCollection.CountDocumentsAsync(filter);
        }

        public async Task CreateAsync(Observation newObservation) => 
            await _observationCollection.InsertOneAsync(newObservation);

        //public async Task UpdateAsync(string observationId, Observation updatedObservation)
        //{
        //    var filter = Builders<Observation>.Filter.And(
        //        Builders<Observation>.Filter.Eq(c => c.ObservationId, observationId),
        //        Builders<Observation>.Filter.Eq(c => c.CropId, updatedObservation.CropId)
        //        );

        //    await _observationCollection.UpdateOneAsync(filter,
        //        Builders<Observation>.Update
        //            .Set(c => c.ObservationType, updatedObservation.ObservationType)
        //            .Set(c => c.Message, updatedObservation.Message)
        //            .Set(c => c.UpdatedAt, DateTime.UtcNow)
        //    );
        //}

        public async Task RemoveAsync(string observationId) =>
            await _observationCollection.DeleteOneAsync(x => x.ObservationId == observationId);
    }
}