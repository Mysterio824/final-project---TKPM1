using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace DevTools.Application.Services
{
    public interface IToolExecutionService
    {
        Task<ToolResponseDto> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role);
    }
}