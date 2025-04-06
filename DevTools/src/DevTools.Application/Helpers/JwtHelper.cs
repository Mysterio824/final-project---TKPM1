using DevTools.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DevTools.Application.Helpers
{
    public static class JwtHelper
    {
        public static Claim[] CreateUserClaims(User user)
        {
            return new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
        }

        public static ClaimsPrincipal? DecodeJwtWithoutValidation(string token)
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
    }
}
