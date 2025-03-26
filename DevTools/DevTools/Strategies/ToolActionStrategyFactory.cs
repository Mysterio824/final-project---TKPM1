using DevTools.Strategies.ToolStrategy;

namespace DevTools.Strategies
{
    public class ToolActionStrategyFactory
    {
        private readonly Dictionary<string, IToolActionStrategy> _strategies;

        public ToolActionStrategyFactory(IEnumerable<IToolActionStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(
                strategy => strategy.GetType().Name.Replace("ToolStrategy", "").ToLower(),
                strategy => strategy,
                StringComparer.OrdinalIgnoreCase);
        }

        public IToolActionStrategy GetStrategy(string actionName)
        {
            if (!_strategies.TryGetValue(actionName.ToLower(), out var strategy))
            {
                throw new ArgumentException("Invalid action. Use 'disable', 'enable', 'setpremium', or 'setfree'.");
            }
            return strategy;
        }
    }
}