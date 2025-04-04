using IToolsApp.Core.Interfaces;
using IToolsApp.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IToolsApp.Core.Services
{
    //    public class ToolLoader
    //    {
    //        private readonly string _toolsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");

    //        public List<Tool> LoadTools()
    //        {
    //            var tools = new List<Tool>();
    //            if (!Directory.Exists(_toolsFolder)) return tools;

    //            foreach (var dll in Directory.GetFiles(_toolsFolder, "*.dll"))
    //            {
    //                try
    //                {
    //                    var assembly = Assembly.LoadFrom(dll);
    //                    foreach (var type in assembly.GetTypes())
    //                    {
    //                        if (typeof(ITool).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
    //                        {
    //                            var toolInstance = Activator.CreateInstance(type) as ITool;
    //                            if (toolInstance != null)
    //                            {
    //                                tools.Add(new Tool
    //                                {
    //                                    Name = toolInstance.Name,
    //                                    Description = toolInstance.Description,
    //                                    Instance = toolInstance
    //                                });
    //                            }
    //                        }
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine($"Error loading tool from {dll}: {ex.Message}");
    //                }
    //            }
    //            return tools;
    //        }
    //    }
    //}
    public class ToolLoader
    {
        private static readonly ToolLoader _instance = new();
        public static ToolLoader Instance => _instance;

        private readonly Dictionary<string, ITool> _tools = new();
        private readonly Dictionary<string, Assembly> _pluginAssemblies = new();

        public event EventHandler<ToolAddedEventArgs> ToolAdded;

        public void RegisterTool(ITool tool)
        {
            if (!_tools.ContainsKey(tool.Name))
            {
                _tools.Add(tool.Name, tool);
                ToolAdded?.Invoke(this, new ToolAddedEventArgs(tool));
            }
        }

        public void LoadPlugin(byte[] dllBytes)
        {
            var assembly = Assembly.Load(dllBytes);
            foreach (var type in assembly.GetTypes()
                .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface))
            {
                if (Activator.CreateInstance(type) is ITool tool)
                {
                    RegisterTool(tool);
                    _pluginAssemblies[tool.Name] = assembly;
                }
            }
        }

        public IEnumerable<ITool> GetAllTools() => _tools.Values;
        public ITool GetTool(string name) => _tools.TryGetValue(name, out var tool) ? tool : null;
    }

    public class ToolAddedEventArgs : EventArgs
    {
        public ITool Tool { get; }
        public ToolAddedEventArgs(ITool tool) => Tool = tool;
    }
}
