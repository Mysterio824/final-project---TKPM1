using DevTools.Application.Common;
using DevTools.Application.Exceptions;
using DevTools.Application.Utils;
using DevTools.Domain.Entities;

namespace DevTools.Application.Helpers
{
    public static class ToolHelper
    {
        public static (Type, ITool) ValidateAndCreateToolInstance(string filePath)
        {
            if (!ToolValidator.IsValidTool(filePath, out Type? toolType) || toolType == null)
            {
                FileHelper.DeleteFile(filePath);
                throw new BadRequestException("Invalid tool DLL. No valid implementation of ITool found.");
            }

            var toolInstance = (ITool?)Activator.CreateInstance(toolType)
                ?? throw new InvalidCastException("Failed to create tool instance.");

            return (toolType, toolInstance);
        }

        public static Tool CreateToolEntity(ITool toolInstance, string filePath)
            => new()
            {
                Name = toolInstance.Name,
                Description = toolInstance.Description,
                DllPath = filePath,
                IsEnabled = true,
                IsPremium = false,
            };
    }
}
