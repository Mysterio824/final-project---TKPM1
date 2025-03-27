using DevTools.Interfaces.Services;

namespace DevTools.Interfaces;

public interface IToolActionStrategy
{
    Task<string> ExecuteAsync(int id);
    string SuccessMessage { get; }
}