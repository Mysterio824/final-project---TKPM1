using DevTools.Application.DTOs.Request;
using DevTools.Application.DTOs.Response;

namespace DevTools.Application.Services
{
    public interface IAuthenticationService
    {
        Task<UserDto?> LoginAsync(LoginDto loginDto);
        Task<string> RefreshTokenAsync(string refreshToken);
        Task LogOutAsync(int userId, string accessToken);
    }
}