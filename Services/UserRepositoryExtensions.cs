using System.Threading.Tasks;
using System.Collections.Generic;
using FarmAPI.Models;

namespace FarmAPI.Services
{
    public static class UserRepositoryExtensions
    {
        public static async Task<User> UpsertByMobileAsync(this UserRepository repo, string mobile)
        {
            var existing = await repo.GetByMobileAsync(mobile);
            if (existing != null) return existing;

            // create a minimal user using CreateUserAsync
            return await repo.CreateUserAsync(mobile, mobile, null, new List<string>());
        }
    }
}
