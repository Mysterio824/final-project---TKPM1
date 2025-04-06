using DevTools.Application.Helpers;
using DevTools.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevTools.Domain.Entities;

namespace DevTools.Application.Services.Impl
{
    public class TokenService(
        IConfiguration configuration, 
        ILogger<TokenService> logger) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<TokenService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public string GenerateAccessToken(User user)
        {
            var claims = JwtHelper.CreateUserClaims(user);
            var token = CreateJwtToken(claims, DateTime.Now.AddMinutes(30));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var token = CreateJwtToken(claims, DateTime.UtcNow.AddDays(7));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string DecodeRefreshToken(string refreshToken)
        {
            var principal = JwtHelper.DecodeJwtWithoutValidation(refreshToken);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedException("Invalid refresh token");

            return userIdClaim;
        }

        private JwtSecurityToken CreateJwtToken(Claim[] claims, DateTime expiration)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT key is not configured.");
                throw new BadRequestException("JWT key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds);
        }
    }
}
