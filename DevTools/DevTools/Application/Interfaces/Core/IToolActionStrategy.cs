namespace DevTools.Application.Interfaces.Core;

public interface IToolActionStrategy
{
    Task<string> ExecuteAsync(int id);
    string SuccessMessage { get; }
}