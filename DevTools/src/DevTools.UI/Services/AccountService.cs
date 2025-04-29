using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DevTools.UI.Models;
using System.Net.Http.Json;

namespace DevTools.UI.Services
{
    public class AccountService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(bool Succeeded, string ErrorMessage)> AddToFavoritesAsync(int toolId)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"Account/favorite/{toolId}/add", content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error adding tool to favorites: {response.StatusCode}");
                    return (false, $"HTTP error: {response.StatusCode}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to add tool to favorites: {error}");
                    return (false, error);
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error adding tool to favorites: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Succeeded, string ErrorMessage)> RemoveFromFavoritesAsync(int toolId)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"Account/favorite/{toolId}/remove", content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error removing tool from favorites: {response.StatusCode}");
                    return (false, $"HTTP error: {response.StatusCode}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to remove tool from favorites: {error}");
                    return (false, error);
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error removing tool from favorites: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Succeeded, string ErrorMessage)> RequestPremiumUpgradeAsync()
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Account/premium/request", content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error requesting premium upgrade: {response.StatusCode}");
                    return (false, $"HTTP error: {response.StatusCode}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to request premium upgrade: {error}");
                    return (false, error);
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error requesting premium upgrade: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Succeeded, string ErrorMessage)> RevokePremiumAsync()
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("Account/premium/revoke", content);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error revoking premium upgrade: {response.StatusCode}");
                    return (false, $"HTTP error: {response.StatusCode}");
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to revoke premium upgrade: {error}");
                    return (false, error);
                }

                return (true, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error revoking premium upgrade: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }
}
