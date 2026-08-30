using FarmAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FarmAPI.Services
{

    public class JwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Actor, user.UserId.ToString()),
                //new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.MobilePhone, user.Mobile),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim("Mobile", user.Mobile),
                new Claim("Actor", user.UserId.ToString()),
                //new Claim("FarmId", user.FarmId)
                new("Actor", user.UserId.ToString())
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}