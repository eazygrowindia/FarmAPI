using FarmAPI.Models;
using FarmAPI.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace FarmAPI.Tests
{
    public class JwtServiceTests
    {
        [Fact]
        public void CreateToken_Returns_NonEmptyToken()
        {
            var settings = Options.Create(new JwtSettings
            {
                Key = "abcdefghijklmnopqrstuvwxyz123",
                Issuer = "test",
                Audience = "test"
            });

            var svc = new JwtService(settings);

            var user = new User { UserId = "u1", Id = "1", Mobile = "+911234567890" };

            var token = svc.CreateToken(user);

            Assert.False(string.IsNullOrEmpty(token));
        }
    }
}
