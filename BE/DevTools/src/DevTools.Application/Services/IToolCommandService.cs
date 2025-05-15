using DevTools.Application.DTOs.Request.Tool;
using DevTools.Application.DTOs.Response;
using DevTools.Application.DTOs.Response.Tool;
using DevTools.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DevTools.Application.Services
{
    public interface IToolCommandService
    {
        Task<CreateToolResponseDto> AddToolAsync(CreateToolDto request);
        Task<UpdateToolResponseDto> UpdateToolAsync(UpdateToolDto request);
        Task<BaseResponseDto> DeleteToolAsync(int id);
        Task<UpdateToolResponseDto> DisableTool(int id);
        Task<UpdateToolResponseDto> EnableTool(int id);
        Task<UpdateToolResponseDto> SetPremium(int id);
        Task<UpdateToolResponseDto> SetFree(int id);
        Task UpdateToolList();
    }
}