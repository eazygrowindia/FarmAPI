using FarmAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace FarmAPI.Services
{
    public class OtpService
    {
        private readonly IMongoCollection<OtpSession> _otps;
        private readonly UserRepository _users;

        public OtpService(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> farmGrowDatabaseSettings, UserRepository users)
        {
            _otps = db.GetCollection<OtpSession>(farmGrowDatabaseSettings.Value.OtpCollectionName);
            _users = users;
        }

        public async Task CreateAndSendOtpAsync(string mobile)
        {
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var hash = Hash(otp);
            var session = new OtpSession
            {
                Mobile = mobile,
                OtpHash = hash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Consumed = false
            };
            await _otps.InsertOneAsync(session);

            // TODO: integrate real SMS gateway here
            Console.WriteLine($"[DEBUG] OTP for {mobile}: {otp}");
        }

        public async Task<User> ValidateOtpAndGetOrCreateUserAsync(string mobile, string otp)
        {
            var now = DateTime.UtcNow;
            var session = await _otps
                .Find(s => s.Mobile == mobile && !s.Consumed && s.ExpiresAt > now)
                .SortByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync();

            if (session == null || session.OtpHash != Hash(otp))
                throw new Exception("Invalid or expired OTP");

            var filter = Builders<OtpSession>.Filter.Eq(s => s.Id, session.Id);
            var update = Builders<OtpSession>.Update.Set(s => s.Consumed, true);
            await _otps.UpdateOneAsync(filter, update);

            return await _users.UpsertByMobileAsync(mobile);
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(bytes);
        }
    }
}
