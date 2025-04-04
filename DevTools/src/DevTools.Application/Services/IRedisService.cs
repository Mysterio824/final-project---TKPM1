using DevTools.Application.DTOs.Request;

namespace DevTools.Application.Services
{
    public interface IRedisService
    {
        Task StoreUnverifiedUserAsync(string email, RegisterDto registerDto, TimeSpan expiration);
        Task<RegisterDto?> GetUnverifiedUserAsync(string email);
        Task RemoveUnverifiedUserAsync(string email);
        Task StoreVerificationTokenAsync(string email, string token, TimeSpan expiration);
        Task<string?> GetEmailByVerificationTokenAsync(string token);
        Task RemoveVerificationTokenAsync(string email);
        Task StoreRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration);
        Task<string?> GetRefreshTokenAsync(string userId);
        Task RemoveRefreshTokenAsync(string userId);
        Task BlacklistAccessTokenAsync(string token, TimeSpan expiration);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}