using DevTools.Application.DTOs.Request.User;
using DevTools.Application.DTOs.Response.User;

namespace DevTools.Application.Services
{
    public interface IRegistrationService
    {
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
        Task<string> VerifyEmailAsync(string token);
    }
}