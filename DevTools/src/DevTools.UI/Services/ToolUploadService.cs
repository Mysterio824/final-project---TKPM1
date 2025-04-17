using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ToolUploadService
    {
        private readonly HttpClient _httpClient;

        public ToolUploadService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
        }

        public async Task<bool> UploadToolAsync(string name, string description, Stream dllStream)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(name), "name");
            content.Add(new StringContent(description), "description");
            content.Add(new StreamContent(dllStream), "file", $"{name}.dll");

            var response = await _httpClient.PostAsync("admin/tools/upload", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EnableToolAsync(int id, bool enable)
        {
            var response = await _httpClient.PostAsJsonAsync($"admin/tools/{id}/set-enabled", new { enabled = enable });
            return response.IsSuccessStatusCode;
        }
    }
}
