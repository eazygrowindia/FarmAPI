using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FarmAPI.Tests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task CreateUser_AddsUser_WhenNotExists()
        {
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockDb = new Mock<IMongoDatabase>();
            mockDb.Setup(d => d.GetCollection<User>(It.IsAny<string>(), null))
                .Returns(mockCollection.Object);

            var settings = Options.Create(new FarmGrowDatabaseSettings
            {
                UsersCollectionName = "Users"
            });

            var mockOwnerService = new Mock<OwnerService>(MockBehavior.Strict, mockDb.Object, settings);
            var mockMaintainerService = new Mock<MaintainerService>(MockBehavior.Strict, mockDb.Object, settings);

            // collection InsertOneAsync will be called - set up a minimal behaviour
            mockCollection.Setup(c => c.InsertOneAsync(It.IsAny<User>(), null, default))
                .Returns(Task.CompletedTask);

            var repo = new UserRepository(mockDb.Object, settings, mockOwnerService.Object, mockMaintainerService.Object);

            var user = await repo.CreateUserAsync("Test", "+911234567890", "t@test.com");

            Assert.Equal("+911234567890", user.Mobile);
            Assert.Equal("Test", user.Name);
        }

        [Fact]
        public async Task GetByMobile_ReturnsNull_WhenNotFound()
        {
            var mockCollection = new Mock<IMongoCollection<User>>();
            var mockDb = new Mock<IMongoDatabase>();
            mockDb.Setup(d => d.GetCollection<User>(It.IsAny<string>(), null))
                .Returns(mockCollection.Object);

            var settings = Options.Create(new FarmGrowDatabaseSettings
            {
                UsersCollectionName = "Users"
            });

            var mockOwnerService = new Mock<OwnerService>(MockBehavior.Strict, mockDb.Object, settings);
            var mockMaintainerService = new Mock<MaintainerService>(MockBehavior.Strict, mockDb.Object, settings);

            var repo = new UserRepository(mockDb.Object, settings, mockOwnerService.Object, mockMaintainerService.Object);

            // Simulate Find/FirstOrDefaultAsync returning null
            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<User>());
            mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<FindOptions<User, User>>(), default))
                .ReturnsAsync(mockCursor.Object);

            var result = await repo.GetByMobileAsync("+911234567890");

            Assert.Null(result);
        }
    }
}
