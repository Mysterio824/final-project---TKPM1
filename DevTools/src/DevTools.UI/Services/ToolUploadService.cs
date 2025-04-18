using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DevTools.UI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevTools.UI.Services
{
    public class ToolUploadService
    {
        private readonly IMockDao _mockDao;
        //private readonly HttpClient _httpClient;

        public ToolUploadService(IMockDao mockDao)
        {
            //_httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri("https://localhost:5000");
            _mockDao = mockDao;
        }

        public async Task<bool> UploadToolAsync(string name, string description, Stream dllStream)
        {
            //var content = new MultipartFormDataContent();
            //content.Add(new StringContent(name), "name");
            //content.Add(new StringContent(description), "description");
            //content.Add(new StreamContent(dllStream), "file", $"{name}.dll");

            //var response = await _httpClient.PostAsync("admin/tools/upload", content);
            //return response.IsSuccessStatusCode;
            var endpoint = "api/tool/add";
            var data = new
            {
                name,
                description,
                file = $"{name}.dll" // Simulate file upload
            };

            var response = await _mockDao.PostAsync<ApiResult<bool>>(endpoint, data);
            return response?.Succeeded ?? false;
        }

        public async Task<bool> EnableToolAsync(int id, bool enable)
        {
            //action. Use 'disable', 'enable', 'setpremium', or 'setfree'."
            //var response = await _httpClient.PostAsJsonAsync($"admin/tools/{id}/set-enabled", new { enabled = enable });
            var endpoint = $"api/tool/{id}/enable";
            var data = new { enabled = enable };
            var response = await _mockDao.PostAsync<ApiResult<bool>>(endpoint, data);
            //return response.IsSuccessStatusCode;
            return response?.Result ?? false;
        }
    }
}
