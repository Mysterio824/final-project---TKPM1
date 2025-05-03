using System.Net;
using DevTools.Application.Common.LinkGenerator;

namespace DevTools.Application.Services.Impl
{
    public class LinkGeneratorService(LinkGenerateSettings settings) : ILinkGeneratorService
    {
        private readonly LinkGenerateSettings _settings = settings;

        public string GenerateEmailVerificationLink(string token)
        {
            return $"{_settings.ApplicationUrl}/api/auth/verify-email?token={token}";
        }

        public string GeneratePasswordResetLink(string token)
        {
            return $"{_settings.ApplicationUrl}/reset-password?code={WebUtility.UrlEncode(token)}";
        }
    }

}
