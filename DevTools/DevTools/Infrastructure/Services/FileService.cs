using DevTools.Application.Interfaces.Services;

namespace DevTools.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _toolDirectory;
        private readonly ILogger<FileService> _logger;

        public FileService(
            ILogger<FileService> logger,
            string toolDirectory = "Tools")
        {
            _toolDirectory = toolDirectory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Directory.CreateDirectory(_toolDirectory);
        }

        public string SaveFile(IFormFile file)
        {
            string filePath = Path.Combine(_toolDirectory, file.FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
            _logger.LogInformation("Saved file to {FilePath}", filePath);
            return filePath;
        }

        public void DeleteFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted file: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to delete file {FilePath}: {ErrorMessage}", filePath, ex.Message);
                    throw;
                }
            }
        }
    }
}