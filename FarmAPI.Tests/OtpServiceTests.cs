using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace FarmAPI.Tests
{
    public class OtpServiceTests
    {
        [Fact]
        public async Task CreateAndSendOtp_InsertsSession()
        {
            var mockCollection = new Mock<IMongoCollection<OtpSession>>();
            var mockDb = new Mock<IMongoDatabase>();
            mockDb.Setup(d => d.GetCollection<OtpSession>(It.IsAny<string>(), null))
                .Returns(mockCollection.Object);

            var settings = Options.Create(new FarmGrowDatabaseSettings
            {
                OtpCollectionName = "OtpSessions",
                ConnectionString = "mongodb://localhost"
            });

            var mockUsers = new Mock<UserRepository>(MockBehavior.Strict, mockDb.Object, null as IOptions<FarmGrowDatabaseSettings>, null as OwnerService, null as MaintainerService);

            mockCollection.Setup(c => c.InsertOneAsync(It.IsAny<OtpSession>(), null, default))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = new OtpService(mockDb.Object, settings, mockUsers.Object);

            await service.CreateAndSendOtpAsync("+911234567890");

            mockCollection.Verify();
        }

        [Fact]
        public async Task ValidateOtp_ReturnsUser_WhenValid()
        {
            var mockCollection = new Mock<IMongoCollection<OtpSession>>();
            var mockDb = new Mock<IMongoDatabase>();
            mockDb.Setup(d => d.GetCollection<OtpSession>(It.IsAny<string>(), null))
                .Returns(mockCollection.Object);

            var settings = Options.Create(new FarmGrowDatabaseSettings
            {
                OtpCollectionName = "OtpSessions",
                ConnectionString = "mongodb://localhost"
            });

            var mockUsers = new Mock<UserRepository>(MockBehavior.Strict, mockDb.Object, null as IOptions<FarmGrowDatabaseSettings>, null as OwnerService, null as MaintainerService);

            // Create a session and set up Find/Sort/FirstOrDefaultAsync chain by mocking the cursor
            var session = new OtpSession
            {
                Mobile = "+911234567890",
                OtpHash = Convert.ToBase64String(new System.Security.Cryptography.SHA256Managed().ComputeHash(System.Text.Encoding.UTF8.GetBytes("123456"))),
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Consumed = false
            };

            var mockCursor = new Mock<IAsyncCursor<OtpSession>>();
            mockCursor.Setup(_ => _.Current).Returns(new[] { session });
            mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<OtpSession>>(), It.IsAny<FindOptions<OtpSession, OtpSession>>(), default))
                .ReturnsAsync(mockCursor.Object);

            mockCollection.Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<OtpSession>>(), It.IsAny<UpdateDefinition<OtpSession>>(), null, default))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var expectedUser = new User { Mobile = "+911234567890", UserId = "u1" };
            mockUsers.Setup(u => u.UpsertByMobileAsync(It.IsAny<string>())).ReturnsAsync(expectedUser);

            var service = new OtpService(mockDb.Object, settings, mockUsers.Object);

            var user = await service.ValidateOtpAndGetOrCreateUserAsync("+911234567890", "123456");

            Assert.Equal(expectedUser, user);
        }
    }
}
