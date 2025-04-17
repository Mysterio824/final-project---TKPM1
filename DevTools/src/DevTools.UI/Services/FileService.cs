using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class FileService
    {
        private readonly HttpClient _httpClient;

        public FileService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
        }

        public async Task<byte[]> DownloadToolDllAsync(int toolId)
        {
            var response = await _httpClient.GetAsync($"tools/{toolId}/download");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
