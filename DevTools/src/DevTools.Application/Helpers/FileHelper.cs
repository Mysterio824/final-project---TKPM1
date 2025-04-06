using Microsoft.AspNetCore.Http;    

namespace DevTools.Application.Helpers
{
    public static class FileHelper
    {
        public static void ValidateToolFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (Path.GetExtension(file.FileName)?.ToLower() != ".dll")
                throw new ArgumentException("Only DLL files are allowed.");
        }
    }
}
