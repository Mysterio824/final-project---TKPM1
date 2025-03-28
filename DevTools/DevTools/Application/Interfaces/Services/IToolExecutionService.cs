using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;

namespace DevTools.Application.Interfaces.Services
{
    public interface IToolExecutionService
    {
        Task<ToolResponse> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role);
    }
}