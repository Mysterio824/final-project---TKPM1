using DevTools.DTOs.Response;
using DevTools.Enums;

namespace DevTools.Interfaces
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