// DevTools/Interfaces/Services/IAuthService.cs
using DevTools.DTOs.UserDtos;

namespace DevTools.Interfaces.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<UserDto> LoginAsync(LoginDto loginDto);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task<bool> VerifyEmailAsync(string token); // Added method
}