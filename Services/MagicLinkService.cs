
using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{
    public class MagicLinkRepository
    {
        private readonly IMongoCollection<MagicLink> _magicLinks;

        public MagicLinkRepository(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> settings)
        {
            _magicLinks = db.GetCollection<MagicLink>(settings.Value.MagicLinksCollectionName);
        }

        public async Task<MagicLink> CreateAsync(string userId, string token, DateTime expiresAt)
        {
            var ml = new MagicLink
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                Used = false
            };
            await _magicLinks.InsertOneAsync(ml);
            return ml;
        }

        public Task<MagicLink?> GetValidByTokenAsync(string token) =>
            _magicLinks.Find(m => m.Token == token && !m.Used && m.ExpiresAt > DateTime.UtcNow)
                       .FirstOrDefaultAsync();

        public async Task MarkUsedAsync(string id)
        {
            var filter = Builders<MagicLink>.Filter.Eq(m => m.Id, id);
            var update = Builders<MagicLink>.Update.Set(m => m.Used, true);
            await _magicLinks.UpdateOneAsync(filter, update);
        }
    }
}