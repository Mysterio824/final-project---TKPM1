using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy;

public interface IToolActionStrategy
{
    string Execute(int id, IToolService toolService);
    string SuccessMessage { get; }
}