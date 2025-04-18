using DevTools.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ToolService
    {
        private readonly HttpClient _httpClient;
        private readonly IMockDao _mockDao;


        public ToolService(IMockDao mockDao)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://localhost:5000");
            _mockDao = mockDao;
        }

        public async Task<List<Tool>> GetToolsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResult<List<Tool>>>("api/tool/all");
            //var response = await _mockDao.GetAsync<ApiResult<List<Tool>>>("api/tool/all");
            return response?.Result ?? new List<Tool>();
        }

        public async Task<Tool> GetToolByIdAsync(int id)
        {
            //var response = await _httpClient.GetFromJsonAsync<ApiResult<Tool>>($"api/tool/{id}");
            var response = await _mockDao.GetAsync<ApiResult<Tool>>($"api/tool/{id}");
            return response?.Result!;
        }
        public async Task<List<Tool>> GetFavouritesAsync()
        {
            //var response = await _httpClient.GetFromJsonAsync<ApiResult<List<Tool>>>("api/tool/favorite/all");
            var response = await _mockDao.GetAsync<ApiResult<List<Tool>>>("api/tool/favorite/all");
            return response?.Result ?? new List<Tool>();
        }

        public async Task<bool> AddToFavouritesAsync(int toolId)
        {
            //var response = await _httpClient.PostAsync($"tool/favorite/{toolId}", null);
            var response = await _mockDao.PostAsync<ApiResult<bool>>($"api/account/favorite/{toolId}/add"); // /remove
            return response?.Result ?? false;
        }
    }
}
