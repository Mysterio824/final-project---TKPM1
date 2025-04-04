namespace DevTools.Application.Strategies.Core;

public interface IToolActionStrategy
{
    Task<string> ExecuteAsync(int id);
    string SuccessMessage { get; }
}