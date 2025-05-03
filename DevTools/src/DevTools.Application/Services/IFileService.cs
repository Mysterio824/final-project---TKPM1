using Microsoft.AspNetCore.Http;

namespace DevTools.Application.Services
{
    public interface IFileService
    {
        string SaveFile(IFormFile file, String name);
        void DeleteFile(string filePath);
    }
}