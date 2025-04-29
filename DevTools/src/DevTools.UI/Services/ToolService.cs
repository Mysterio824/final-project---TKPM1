using DevTools.UI.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ToolService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ToolService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Tool>> GetAllToolsAsync()
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync("Tool/all");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<Tool>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Failed to get tools: {string.Join(", ", apiResponse.Errors)}");
                    return new List<Tool>();
                }

                var tools = new List<Tool>();
                foreach (var item in apiResponse.Result)
                {
                    tools.Add(new Tool
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Name = item.GetProperty("name").GetString(),
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        IsPremium = item.GetProperty("isPremium").GetBoolean(),
                        IsEnabled = item.GetProperty("isEnabled").GetBoolean(),
                        IsFavorite = item.GetProperty("isFavorite").GetBoolean()
                    });
                }

                return tools;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting all tools: {ex.Message}");
                return new List<Tool>();
            }
        }

        public async Task<Tool> GetToolByIdAsync(int id)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync($"Tool/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Failed to get tool: {string.Join(", ", apiResponse.Errors)}");
                    return null;
                }

                var item = apiResponse.Result;

                var tool = new Tool
                {
                    Id = id,
                    Name = item.GetProperty("name").GetString(),
                    Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    IsPremium = item.GetProperty("isPremium").GetBoolean(),
                    IsEnabled = item.GetProperty("isEnabled").GetBoolean(),
                    IsFavorite = item.GetProperty("isFavorite").GetBoolean()
                };

                // Get file bytes
                if (item.TryGetProperty("file", out var fileElement))
                {
                    string base64 = fileElement.GetString();
                    tool.FileData = Convert.FromBase64String(base64);
                }

                return tool;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting tool by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteToolAsync(int id)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.DeleteAsync($"Tool/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error deleting tool: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Tool>> SearchToolsAsync(string name)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync($"Tool/search?name={Uri.EscapeDataString(name)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<Tool>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Search failed: {string.Join(", ", apiResponse.Errors)}");
                    return new List<Tool>();
                }

                var tools = new List<Tool>();
                foreach (var item in apiResponse.Result)
                {
                    tools.Add(new Tool
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Name = item.GetProperty("name").GetString(),
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        IsPremium = item.GetProperty("isPremium").GetBoolean(),
                        IsEnabled = item.GetProperty("isEnabled").GetBoolean(),
                        IsFavorite = item.GetProperty("isFavorite").GetBoolean()
                    });
                }

                return tools;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error searching tools: {ex.Message}");
                return new List<Tool>();
            }
        }

        public async Task<List<Tool>> GetFavoriteToolsAsync()
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync("Tool/favorite/all");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<Tool>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Failed to get favorite tools: {string.Join(", ", apiResponse.Errors)}");
                    return new List<Tool>();
                }

                var tools = new List<Tool>();
                foreach (var item in apiResponse.Result)
                {
                    tools.Add(new Tool
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Name = item.GetProperty("name").GetString(),
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        IsPremium = item.GetProperty("isPremium").GetBoolean(),
                        IsEnabled = item.GetProperty("isEnabled").GetBoolean(),
                        IsFavorite = true // These are favorite tools
                    });
                }

                return tools;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting favorite tools: {ex.Message}");
                return new List<Tool>();
            }
        }

        // Admin methods
        public async Task<int> AddToolAsync(string name, string description, bool isPremium, int groupId, bool isEnabled, IFormFile file)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(name), "Name");
                content.Add(new StringContent(description ?? string.Empty), "Description");
                content.Add(new StringContent(isPremium.ToString()), "IsPremium");
                content.Add(new StringContent(groupId.ToString()), "GroupId");
                content.Add(new StringContent(isEnabled.ToString()), "IsEnabled");

                // Add file
                using var fileStream = file.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                content.Add(fileContent, "File", file.FileName);

                var response = await _httpClient.PostAsync("Tool/add", content);

                if (!response.IsSuccessStatusCode)
                {
                    return -1;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Adding tool failed: {string.Join(", ", apiResponse.Errors)}");
                    return -1;
                }

                return apiResponse.Result.GetProperty("id").GetInt32();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error adding tool: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateToolStatusAsync(int id, string action)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.PatchAsync($"Tool/{id}/{action}", null);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error updating tool status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EditToolAsync(int id, string name, string description, bool isPremium, int groupId, bool isEnabled, IFormFile file = null)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(id.ToString()), "Id");
                content.Add(new StringContent(name), "Name");
                content.Add(new StringContent(description ?? string.Empty), "Description");
                content.Add(new StringContent(isPremium.ToString()), "IsPremium");
                content.Add(new StringContent(groupId.ToString()), "GroupId");
                content.Add(new StringContent(isEnabled.ToString()), "IsEnabled");

                // Add file if provided
                if (file != null)
                {
                    using var fileStream = file.OpenReadStream();
                    var fileContent = new StreamContent(fileStream);
                    content.Add(fileContent, "File", file.FileName);
                }

                var response = await _httpClient.PutAsync("Tool/edit", content);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error editing tool: {ex.Message}");
                return false;
            }
        }
    }
}
