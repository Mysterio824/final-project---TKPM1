using DevTools.Interfaces.Services;
using DevTools.Strategies.ToolStrategy;

namespace DevTools.Strategies;

public class ToolActionStrategyFactory
{
    private readonly IToolService _toolService;
    private readonly IDictionary<string, IToolActionStrategy> _strategies;

    public ToolActionStrategyFactory(IToolService toolService)
    {
        _toolService = toolService;
        _strategies = new Dictionary<string, IToolActionStrategy>(StringComparer.OrdinalIgnoreCase)
        {
            { "disable", new DisableToolStrategy() },
            { "enable", new EnableToolStrategy() },
            { "setpremium", new SetPremiumToolStrategy() },
            { "setfree", new SetFreeToolStrategy() }
        };
    }

    public IToolActionStrategy GetStrategy(string actionName)
    {
        if (string.IsNullOrEmpty(actionName) || !_strategies.TryGetValue(actionName, out var strategy))
        {
            throw new ArgumentException("Invalid action. Use 'disable', 'enable', 'setpremium', or 'setfree'.");
        }
        return strategy;
    }
}