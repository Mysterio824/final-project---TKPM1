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
        //private readonly HttpClient _httpClient;
        private readonly IMockDao _mockDao;

        public FileService(IMockDao mockDao)
        {
            //_httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri("https://localhost:5000");
            _mockDao = mockDao;
        }

        public async Task<byte[]> DownloadToolDllAsync(int toolId)
        {
            //var response = await _httpClient.GetAsync($"tools/{toolId}/download");
            //response.EnsureSuccessStatusCode();
            //return await response.Content.ReadAsByteArrayAsync();
            var response = await _mockDao.GetAsync<byte[]>($"api/tool/{toolId}");
            if (response == null)
            {
                throw new Exception("Failed to download tool DLL.");
            }
            return response;
        }
    }
}
