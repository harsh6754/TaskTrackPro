using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Server.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string AccessToken, string RefreshToken, DateTime RefreshExpiry) GenerateTokens(string userId)
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("UserId", userId),
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing.")));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Ensure values are not null and default to sensible values if missing
            int accessTokenExpiry = int.TryParse(_config["Jwt:AccessTokenExpiry"], out var atExp) ? atExp : 60;
            int refreshTokenExpiry = int.TryParse(_config["Jwt:RefreshTokenExpiry"], out var rtExp) ? rtExp : 1440;

            var accessToken = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenExpiry),
                signingCredentials: signIn
            );

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            DateTime refreshExpiry = DateTime.UtcNow.AddMinutes(refreshTokenExpiry);

            return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken, refreshExpiry);
        }

    }
}