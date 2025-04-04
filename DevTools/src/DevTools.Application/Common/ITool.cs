using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;

namespace DevTools.Application.Common
{
    public interface ITool
    {
        string Name { get; }

        string Description { get; }

        ToolType Type { get; }

        ToolResponseDto Execute(string input);

        ToolResponseDto Execute(byte[] fileBytes);
    }
}