using DevTools.Application.DTOs.Request;
using DevTools.Application.DTOs.Response;

namespace DevTools.Application.Interfaces.Services
{
    public interface IRegistrationService
    {
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
        Task<bool> VerifyEmailAsync(string token);
    }
}