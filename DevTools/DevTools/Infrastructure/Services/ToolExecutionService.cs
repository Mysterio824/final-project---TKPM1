using DevTools.Application.Common;
using DevTools.Application.DTOs.Response;
using DevTools.Application.Interfaces.Repositories;
using DevTools.Application.Interfaces.Services;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using System.Reflection;

namespace DevTools.Infrastructure.Services
{
    public class ToolExecutionService : IToolExecutionService
    {
        private readonly IToolRepository _toolRepository;
        private readonly ILogger<ToolExecutionService> _logger;

        public ToolExecutionService(
            IToolRepository toolRepository,
            ILogger<ToolExecutionService> logger)
        {
            _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ToolResponse> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role)
        {
            ValidateToolExecution(input, file);
            var tool = await GetAndValidateTool(toolId, role);

            var assembly = Assembly.LoadFrom(tool.DllPath);
            var toolType = GetToolType(assembly);
            var toolInstance = CreateToolInstance(toolType);

            return await ExecuteTool(toolInstance, input, file, toolId);
        }

        private static void ValidateToolExecution(string? input, IFormFile? file)
        {
            if (input == null && file == null)
                throw new ArgumentException("No input provided.");
        }

        private async Task<Tool> GetAndValidateTool(int toolId, UserRole role)
        {
            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null || !File.Exists(tool.DllPath))
                throw new ArgumentException("Tool not found or missing DLL.");

            ValidateToolAccess(tool, role);
            return tool;
        }

        private static void ValidateToolAccess(Tool tool, UserRole role)
        {
            if (!tool.IsEnabled)
                throw new UnauthorizedAccessException("Tool is disabled.");

            if (tool.IsPremium && (role == UserRole.User || role == UserRole.Anonymous))
                throw new UnauthorizedAccessException("Tool is premium.");
        }

        private static Type GetToolType(Assembly assembly)
            => assembly.GetTypes()
                .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                ?? throw new InvalidOperationException("Invalid tool DLL: No ITool implementation found.");

        private static ITool CreateToolInstance(Type toolType)
            => (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

        private static async Task<ToolResponse> ExecuteTool(ITool toolInstance, string? input, IFormFile? file, int toolId)
        {
            ToolResponse response = file != null
                ? await ExecuteToolWithFile(toolInstance, file)
                : ExecuteToolWithInput(toolInstance, input!);

            response.ToolId = toolId;
            return response;
        }

        private static async Task<ToolResponse> ExecuteToolWithFile(ITool toolInstance, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return toolInstance.Execute(fileBytes);
        }

        private static ToolResponse ExecuteToolWithInput(ITool toolInstance, string input)
            => toolInstance.Execute(input);
    }
}