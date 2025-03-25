using DevTools.DTOs.Request;
using DevTools.DTOs.Response;

namespace DevTools.Interfaces.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<UserDto> LoginAsync(LoginDto loginDto);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task<bool> VerifyEmailAsync(string token);
    Task LogOutAsync(int userId, string accessToken);
}