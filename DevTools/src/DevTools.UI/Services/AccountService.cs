using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AccountService(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public void SetAuthToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<bool> AddToFavoritesAsync(int toolId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Account/favorite/{toolId}/add", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error adding tool to favorites: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int toolId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Account/favorite/{toolId}/remove", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error removing tool from favorites: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RequestPremiumUpgradeAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Account/premium/request", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error requesting premium upgrade: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokePremiumAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Account/premium/revoke", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error revoking premium status: {ex.Message}");
                return false;
            }
        }
    }
}
