using DevTools.Application.DTOs.Response;
using DevTools.Domain.Enums;

namespace DevTools.Application.Common
{
    public interface ITool
    {
        string Name { get; }

        string Description { get; }

        ToolType Type { get; }

        ToolResponse Execute(string input);

        ToolResponse Execute(byte[] fileBytes);
    }
}