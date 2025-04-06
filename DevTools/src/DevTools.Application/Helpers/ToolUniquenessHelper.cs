using DevTools.Domain.Entities;
using DevTools.Infrastructure.Repositories;
using DevTools.Application.Exceptions;
using DevTools.Application.Services;
using DevTools.Application.Utils;

namespace DevTools.Application.Helpers
{
    public static class ToolUniquenessHelper
    {
        public static async Task EnsureToolIsUnique(Tool newTool, string filePath, IToolRepository toolRepository, IFileService fileService)
        {
            var existingTools = await toolRepository.GetAll();
            if (existingTools.Contains(newTool, new ToolComparer()))
            {
                fileService.DeleteFile(filePath);
                throw new BadRequestException("A tool with the same name already exists.");
            }
        }
    }
}
