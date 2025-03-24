// DevTools/Interfaces/Services/IRedisService.cs
using DevTools.DTOs.Request;

namespace DevTools.Interfaces.Services
{
    public interface IRedisService
    {
        Task StoreUnverifiedUserAsync(string email, RegisterDto registerDto, TimeSpan expiration);
        Task<RegisterDto> GetUnverifiedUserAsync(string email);
        Task RemoveUnverifiedUserAsync(string email);
        Task StoreVerificationTokenAsync(string email, string token, TimeSpan expiration);
        Task<string> GetVerificationTokenAsync(string email); // Keep for other uses
        Task<string> GetEmailByVerificationTokenAsync(string token); // Add this
        Task RemoveVerificationTokenAsync(string email);
        Task StoreRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration);
        Task<string> GetRefreshTokenAsync(string userId);
        Task RemoveRefreshTokenAsync(string userId);
    }
}