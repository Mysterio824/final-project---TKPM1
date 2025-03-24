using DevTools.Enums;

namespace DevTools.Interfaces.Services
{
    public interface ITool
    {
        string Name { get; }
        string Description { get; }

        ToolType Type { get; }
        string Execute(string input);
    }
}
