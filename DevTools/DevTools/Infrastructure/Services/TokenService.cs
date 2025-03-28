using DevTools.Application.Interfaces.Services;
using DevTools.Domain.Entities;
using DevTools.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevTools.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GenerateAccessToken(User user)
        {
            var claims = CreateUserClaims(user);
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
            var principal = DecodeJwtWithoutValidation(refreshToken);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedException("Invalid refresh token");

            return userIdClaim;
        }

        private static ClaimsPrincipal? DecodeJwtWithoutValidation(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    SignatureValidator = (t, p) => tokenHandler.ReadToken(t)
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private static Claim[] CreateUserClaims(User user)
            => new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

        private JwtSecurityToken CreateJwtToken(Claim[] claims, DateTime expiration)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT key is not configured.");
                throw new InvalidOperationException("JWT key is not configured.");
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