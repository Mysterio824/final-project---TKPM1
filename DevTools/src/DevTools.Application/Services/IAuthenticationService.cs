using DevTools.Application.DTOs.Request.User;
using DevTools.Application.DTOs.Response.User;

namespace DevTools.Application.Services
{
    public interface IAuthenticationService
    {
        Task<UserDto?> LoginAsync(LoginDto loginDto);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task LogOutAsync(int userId, string accessToken);
    }
}