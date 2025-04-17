using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Utils
{
    public static class JwtTokenManager
    {
        private static string? _token;

        public static void SaveToken(string token) => _token = token;

        public static string? GetToken() => _token;

        public static void ClearToken() => _token = null;

        public static bool IsLoggedIn => !string.IsNullOrEmpty(_token);

        public static bool IsAdmin
        {
            get
            {
                var claim = GetClaim("role");
                return claim == "admin";
            }
        }

        public static bool IsPremium
        {
            get
            {
                var claim = GetClaim("is_premium");
                return claim == "true";
            }
        }

        private static string? GetClaim(string claimType)
        {
            if (_token == null) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(_token);

            return jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }
}
