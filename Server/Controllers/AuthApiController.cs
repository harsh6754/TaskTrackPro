using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private static ConcurrentDictionary<string, (string userId, DateTime Expiry)> _refreshTokens = new();
        private readonly JwtTokenService _jwtTokenService;

        public AuthApiController(JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromForm] t_Login login)
        {
            // TODO: Replace this with actual user authentication from the database
            if (login.c_email == login.c_email && login.c_password == login.c_password)
            {
                var tokens = _jwtTokenService.GenerateTokens(login.c_email);

                // Remove old refresh token for this user
                var existingToken = _refreshTokens.FirstOrDefault(x => x.Value.userId == login.c_email);
                if (!string.IsNullOrEmpty(existingToken.Key))
                {
                    _refreshTokens.TryRemove(existingToken.Key, out _);
                }

                // Store new refresh token
                _refreshTokens.TryAdd(tokens.RefreshToken, (login.c_email, tokens.RefreshExpiry));

                return Ok(new
                {
                    accessToken = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken
                });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromForm] string refreshToken)
        {
            if (!_refreshTokens.TryGetValue(refreshToken, out var tokenDetails))
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // Check if refresh token has expired
            if (tokenDetails.Expiry < DateTime.UtcNow)
            {
                _refreshTokens.TryRemove(refreshToken, out _);
                return Unauthorized(new { message = "Refresh token expired" });
            }

            // Generate new tokens
            var newTokens = _jwtTokenService.GenerateTokens(tokenDetails.userId);

            // Remove old refresh token
            _refreshTokens.TryRemove(refreshToken, out _);

            // Store new refresh token
            _refreshTokens.TryAdd(newTokens.RefreshToken, (tokenDetails.userId, newTokens.RefreshExpiry));

            return Ok(new
            {
                accessToken = newTokens.AccessToken,
                refreshToken = newTokens.RefreshToken
            });
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedRoute()
        {
            return Ok(new { message = "You have accessed a protected route!" });
        }
    }
}
