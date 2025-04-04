using DevTools.Application.Common;
using DevTools.Application.DTOs.Response;
using DevTools.Infrastructure.Repositories;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using DevTools.Application.Exceptions;

namespace DevTools.Application.Services.Impl
{
    public class ToolExecutionService(
        IToolRepository toolRepository) : IToolExecutionService
    {
        private readonly IToolRepository _toolRepository = toolRepository ?? throw new ArgumentNullException(nameof(toolRepository));

        public async Task<ToolResponseDto> ExecuteToolAsync(int toolId, string? input, IFormFile? file, UserRole role)
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
                ?? throw new BadRequestException("Invalid tool DLL: No ITool implementation found.");

        private static ITool CreateToolInstance(Type toolType)
            => (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

        private static async Task<ToolResponseDto> ExecuteTool(ITool toolInstance, string? input, IFormFile? file, int toolId)
        {
            ToolResponseDto response = file != null
                ? await ExecuteToolWithFile(toolInstance, file)
                : ExecuteToolWithInput(toolInstance, input!);

            response.ToolId = toolId;
            return response;
        }

        private static async Task<ToolResponseDto> ExecuteToolWithFile(ITool toolInstance, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return toolInstance.Execute(fileBytes);
        }

        private static ToolResponseDto ExecuteToolWithInput(ITool toolInstance, string input)
            => toolInstance.Execute(input);
    }
}