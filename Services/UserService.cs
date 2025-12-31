using FarmAPI.Models;
using FarmAPI.Models.Dtos;
using FarmAPI.Utils;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FarmAPI.Services
{

    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;
        private OwnerService _ownerService;
        private MaintainerService _maintainerService;

        public UserRepository(IMongoDatabase db, IOptions<FarmGrowDatabaseSettings> settings, OwnerService ownerService, MaintainerService maintainerService)
        {
            _users = db.GetCollection<User>(settings.Value.UsersCollectionName);
            _ownerService = ownerService;
            _maintainerService = maintainerService;
        }

        public Task<List<User>?> GetAllAsync() =>
            _users.Find(_ => true).ToListAsync();

        public Task<User?> GetByMobileAsync(string mobile) =>
            _users.Find(u => u.Mobile == mobile).FirstOrDefaultAsync();

        public Task<User?> GetByEmailAsync(string email) =>
            _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public Task<User?> GetByUserIdAsync(string userId) =>
            _users.Find(u => u.UserId == userId).FirstOrDefaultAsync();

        public Task<User?> GetByIdAsync(string id) =>
            _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public Task<User?> GetByCredentialIdAsync(string credentialId) =>
            _users.Find(u => u.WebAuthnCredentials.Any(c => c.CredentialId == credentialId))
                  .FirstOrDefaultAsync();

        public async Task UpdatePasswordAsync(string userId, string hash, string salt)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, hash)
                .Set(u => u.PasswordSalt, salt)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Used by Register functionality to create a user
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<User> CreateUserAsync(string name, string mobile, string? email = null, List<string>? roles = null)
        {
            //REVIEW: This logic can be revisited by UnitOfWork pattern for a cleaner approach
            // This logic needs a transactional approach as multiple mongo collection update * insert are involved
            // Any error in between will put the collections out of sync

            var newRoles = new List<string>();
            foreach(var role in roles)
            {
                newRoles.Add(UserRolesHelper.GetRole(role).ToString());
            }

            //create user
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Name = name,
                Mobile = mobile,
                Email = email,
                EmailVerified = false,
                SystemStatus = SystemStatus.ACTIVE.ToString(),
                //Roles = new List<string> { UserRole.ToRoleString(roles) }
                Roles = newRoles.Count > 0 ? newRoles : new List<string>()
            };

            //add logic to check if mobile number matches owner/maintainer
            var matchedOwner = await _ownerService.GetAsyncByMobile(mobile);
            if(matchedOwner != null)
            {
                matchedOwner.UserId = user.UserId; //linked to the owner
                if (!user.Roles.Contains(UserRoles.FARMOWNER.ToString())) 
                    user.Roles.Add(UserRoles.FARMOWNER.ToString());

                user.Roles.Remove(UserRoles.FARMHELP.ToString());
                user.Roles.Remove(UserRoles.UNKNOWN.ToString());

                var filter = Builders<Owner>.Filter.Eq(o => o.OwnerId, matchedOwner.OwnerId);
                var update = Builders<Owner>.Update.Set(o => o.UserId, matchedOwner.UserId);
                await _ownerService.UpdateAsyncByFilterUpdateDefinitions(filter, update);

            }
            else
            {
                user.Roles.Remove(UserRoles.FARMOWNER.ToString());
            }

                var matchedMaintainer = await _maintainerService.GetAsyncByMobile(mobile);
            if (matchedMaintainer != null)
            {
                matchedMaintainer.UserId = user.UserId;    //linked to the maintainer/farmhelp
                if (!user.Roles.Contains(UserRoles.FARMHELP.ToString()))
                    user.Roles.Add(UserRoles.FARMHELP.ToString());

                user.Roles.Remove(UserRoles.FARMOWNER.ToString());
                user.Roles.Remove(UserRoles.UNKNOWN.ToString());

                var filter = Builders<Maintainer>.Filter.Eq(o => o.MaintainerId, matchedMaintainer.MaintainerId);
                var update = Builders<Maintainer>.Update.Set(o => o.UserId, matchedMaintainer.UserId);
                await _maintainerService.UpdateAsyncByFilterUpdateDefinitions(filter, update);
            }
            else
            {
                user.Roles.Remove(UserRoles.FARMHELP.ToString());
            }

            await _users.InsertOneAsync(user);

            return user;
        }

        public async Task<User> UpdateUserRolesAsync(UpdateUserDto updateUserDto, User existingUser)
        {
            //add logic to check if mobile number matches owner/maintainer
            var matchedOwner = await _ownerService.GetAsyncByMobile(updateUserDto.Mobile);
            if (matchedOwner != null)
            {
                if (!updateUserDto.Roles.Contains(UserRoles.FARMOWNER.ToString()))
                    updateUserDto.Roles.Add(UserRoles.FARMOWNER.ToString());

                updateUserDto.Roles.Remove(UserRoles.FARMHELP.ToString());
                updateUserDto.Roles.Remove(UserRoles.UNKNOWN.ToString());

            }
            else
            {
                updateUserDto.Roles.Remove(UserRoles.FARMOWNER.ToString());
            }

            var matchedMaintainer = await _maintainerService.GetAsyncByMobile(updateUserDto.Mobile);
            if (matchedMaintainer != null)
            {
                if (!updateUserDto.Roles.Contains(UserRoles.FARMHELP.ToString()))
                    updateUserDto.Roles.Add(UserRoles.FARMHELP.ToString());

                updateUserDto.Roles.Remove(UserRoles.FARMOWNER.ToString());
                updateUserDto.Roles.Remove(UserRoles.UNKNOWN.ToString());
            }
            else
            {
                updateUserDto.Roles.Remove(UserRoles.FARMHELP.ToString());
            }

            if(updateUserDto.Roles.Any())
                existingUser.Roles = updateUserDto.Roles;

            if (!existingUser.Roles.Any())
                existingUser.Roles.Add(UserRoles.UNKNOWN.ToString());

            return existingUser;
        }

        public async Task<bool> UpdateMobileReferences(User existingUser, string newMobileNumber)
        {
            bool isUpdated = false;
            if (existingUser.Roles.Contains(UserRoles.FARMOWNER.ToString()))
            {
                var matchedOwner = await _ownerService.GetAsyncByMobile(existingUser.Mobile);
                if (matchedOwner != null)
                {
                    matchedOwner.ContactNumber = newMobileNumber; //update the mobile number
                    var filter = Builders<Owner>.Filter.Eq(o => o.OwnerId, matchedOwner.OwnerId);
                    var update = Builders<Owner>.Update.Set(o => o.ContactNumber, matchedOwner.ContactNumber);
                    await _ownerService.UpdateAsyncByFilterUpdateDefinitions(filter, update);
                    existingUser.Mobile = newMobileNumber;
                    isUpdated = true;
                }
            }

            if (existingUser.Roles.Contains(UserRoles.FARMHELP.ToString()))
            {
                var matchedMaintainer = await _maintainerService.GetAsyncByMobile(existingUser.Mobile);
                if (matchedMaintainer != null)
                {
                    matchedMaintainer.ContactNumber = newMobileNumber; //update the mobile number
                    var filter = Builders<Maintainer>.Filter.Eq(o => o.MaintainerId, matchedMaintainer.MaintainerId);
                    var update = Builders<Maintainer>.Update.Set(o => o.ContactNumber, matchedMaintainer.ContactNumber);
                    await _maintainerService.UpdateAsyncByFilterUpdateDefinitions(filter, update);
                    existingUser.Mobile = newMobileNumber;
                    isUpdated = true;
                }
            }

            return isUpdated;
        }

        public async Task UpdateLastLoginAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task UpdateSystemStatusAsync(string userId, string systemStatus)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<User>.Update.Set(u => u.SystemStatus, systemStatus)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task AddWebAuthnCredentialAsync(string userId, WebAuthnCredential cred)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<User>.Update.Push(u => u.WebAuthnCredentials, cred);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task SetEmailVerifiedAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<User>.Update.Set(u => u.EmailVerified, true);
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateAsync(string userId, User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(x => x.UserId, userId);

            var result = await _users.UpdateOneAsync(
                filter,
                Builders<User>.Update
                    .Set(f => f.Name, updatedUser.Name)
                    .Set(f => f.Email, updatedUser.Email)
                    .Set(f => f.Roles, updatedUser.Roles)
                    .Set(f => f.SystemStatus, updatedUser.SystemStatus)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow)
            );

            return result;
        }

        public async Task<UpdateResult> UpdateUserProfileAsync(string userId, User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(x => x.UserId, userId);

            var result = await _users.UpdateOneAsync(
                filter,
                Builders<User>.Update
                    .Set(f => f.Name, updatedUser.Name)
                    .Set(f => f.Email, updatedUser.Email)
                    //.Set(f => f.Mobile, updatedUser.Mobile)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow)
            );

            return result;
        }

        public async Task RemoveAsync(string userId) =>
            await _users.DeleteOneAsync(x => x.UserId == userId);
    }
}