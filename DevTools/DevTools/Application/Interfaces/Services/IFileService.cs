namespace DevTools.Application.Interfaces.Services
{
    public interface IFileService
    {
        string SaveFile(IFormFile file);
        void DeleteFile(string filePath);
    }
}