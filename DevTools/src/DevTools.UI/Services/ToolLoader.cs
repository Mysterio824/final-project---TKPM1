using DevTools.UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ToolLoader
    {
        private static readonly ToolLoader _instance = new();
        public static ToolLoader Instance => _instance;

        private readonly Dictionary<string, ITool> _tools = new();
        private readonly Dictionary<string, Assembly> _pluginAssemblies = new();

        public event EventHandler<ToolAddedEventArgs> ToolAdded;

        public void RegisterTool(Tool tool, ITool toolInstance)
        {
            string key = tool.Id.ToString(); // Use Tool.Id as the key
            if (!_tools.ContainsKey(key))
            {
                _tools.Add(key, toolInstance);
                ToolAdded?.Invoke(this, new ToolAddedEventArgs(toolInstance));
            }
        }

        public ITool LoadPlugin(Tool tool, byte[] dllBytes)
        {
            try
            {
                var assembly = Assembly.Load(dllBytes);
                foreach (var type in assembly.GetTypes()
                    .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
                {
                    if (Activator.CreateInstance(type) is ITool toolInstance)
                    {
                        RegisterTool(tool, toolInstance);
                        _pluginAssemblies[tool.Id.ToString()] = assembly;
                        return toolInstance;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading plugin: {ex}");
                return null;
            }
        }

        public IEnumerable<ITool> GetAllTools() => _tools.Values;
        public ITool GetTool(string id) => _tools.TryGetValue(id, out var tool) ? tool : null;
    }

    public class ToolAddedEventArgs : EventArgs
    {
        public ITool Tool { get; }
        public ToolAddedEventArgs(ITool tool) => Tool = tool;
    }
}
