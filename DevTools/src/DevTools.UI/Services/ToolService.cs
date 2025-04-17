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
        //private readonly HttpClient _httpClient;
        private readonly IMockDao _mockDao;

        public ToolService(IMockDao mockDao)
        {
            //_httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri("http://localhost:5000");
            _mockDao = mockDao;
        }

        public async Task<List<Tool>> GetToolsAsync()
        {
            //var response = await _httpClient.GetFromJsonAsync<ApiResult<List<Tool>>>("tools/all");
            var response = await _mockDao.GetAsync<ApiResult<List<Tool>>>("tools/all");
            return response?.Result ?? new List<Tool>();
        }

        public async Task<Tool> GetToolByIdAsync(int id)
        {
            //var response = await _httpClient.GetFromJsonAsync<ApiResult<Tool>>($"tools/{id}");
            var response = await _mockDao.GetAsync<ApiResult<Tool>>($"tools/{id}");
            return response?.Result!;
        }
        public async Task<List<Tool>> GetFavouritesAsync()
        {
            //var response = await _httpClient.GetFromJsonAsync<ApiResult<List<Tool>>>("tools/favorites");
            var response = await _mockDao.GetAsync<ApiResult<List<Tool>>>("tools/favorites");
            return response?.Result ?? new List<Tool>();
        }

        public async Task<bool> AddToFavouritesAsync(int toolId)
        {
            //var response = await _httpClient.PostAsync($"tools/favorite/{toolId}", null);
            var response = await _mockDao.PostAsync<ApiResult<bool>>($"tools/favorite/{toolId}");
            return response?.Result ?? false;
        }
    }
}
