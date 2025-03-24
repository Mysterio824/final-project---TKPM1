using DevTools.Entities;
using DevTools.Enums;
using DevTools.Interfaces.Repositories;
using DevTools.Interfaces.Services;
using System.Reflection;

namespace DevTools.Services
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _toolRepository;
        private readonly string _toolDirectory = "Tools";

        public ToolService(IToolRepository toolRepository)
        {
            _toolRepository = toolRepository;

            if (!Directory.Exists(_toolDirectory))
                Directory.CreateDirectory(_toolDirectory);
        }

        public async Task<IEnumerable<Tool>> GetToolsAsync()
        {
            return await _toolRepository.GetAllAsync();
        }

        public async Task<Tool?> GetToolByIdAsync(int id)
        {
            return await _toolRepository.GetByIdAsync(id);
        }

        public async Task AddToolAsync(Tool tool)
        {
            await _toolRepository.AddAsync(tool);
        }

        public async Task UpdateToolAsync(Tool tool)
        {
            await _toolRepository.UpdateAsync(tool);
        }

        public async Task DeleteToolAsync(int id)
        {
            await _toolRepository.DeleteAsync(id);
        }

        /// ✅ **Fetch tool DLLs and update DB**
        public async Task UpdateToolList()
        {
            var dllFiles = Directory.GetFiles(_toolDirectory, "*.dll");

            foreach (var dllPath in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dllPath);
                var toolType = assembly.GetTypes().FirstOrDefault(t =>
                    typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (toolType == null) continue;

                var toolInstance = (ITool)Activator.CreateInstance(toolType);

                var existingTool = await _toolRepository.GetAllAsync();
                if (!existingTool.Any(t => t.DllPath == dllPath))
                {
                    var newTool = new Tool
                    {
                        Name = toolInstance.Name,
                        Description = toolInstance.Description,
                        DllPath = dllPath,
                        IsEnabled = true,
                        IsPremium = false,
                        type = ToolType.String // You may want to get this from the DLL dynamically
                    };

                    await _toolRepository.AddAsync(newTool);
                }
            }
        }


        /// ✅ **Execute a tool dynamically from DLL**
        public string ExecuteTool(int toolId, string input)
        {
            var tool = _toolRepository.GetByIdAsync(toolId).Result;
            if (tool == null || !File.Exists(tool.DllPath))
                throw new Exception("Tool not found or missing DLL.");

            var assembly = Assembly.LoadFrom(tool.DllPath);

            // ✅ Find the class that implements an interface named `ITool`
            var toolType = assembly.GetTypes().FirstOrDefault(t =>
                t.GetInterfaces().Any(i => i.Name == "ITool") && !t.IsInterface && !t.IsAbstract);

            if (toolType == null)
                throw new Exception("Invalid tool DLL: No ITool implementation found.");

            var toolInstance = Activator.CreateInstance(toolType);
            var executeMethod = toolType.GetMethod("Execute");

            if (executeMethod == null)
                throw new Exception("Invalid tool DLL: Missing Execute method.");

            return (string)executeMethod.Invoke(toolInstance, new object[] { input });
        }
    }
}
