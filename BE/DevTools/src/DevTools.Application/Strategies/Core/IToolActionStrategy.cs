using DevTools.Application.DTOs.Response.Tool;

namespace DevTools.Application.Strategies.Core;

public interface IToolActionStrategy
{
    Task<UpdateToolResponseDto> ExecuteAsync(int id);
}