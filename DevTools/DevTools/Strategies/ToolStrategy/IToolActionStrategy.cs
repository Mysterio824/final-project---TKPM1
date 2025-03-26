using DevTools.Interfaces.Services;

namespace DevTools.Strategies.ToolStrategy;

public interface IToolActionStrategy
{
    Task<string> ExecuteAsync(int id);
    string SuccessMessage { get; }
}