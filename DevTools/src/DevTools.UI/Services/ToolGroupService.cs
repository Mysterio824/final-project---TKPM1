using DevTools.UI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DevTools.UI.Services
{
    public class ToolGroupService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ToolGroupService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ToolGroup>> GetAllToolGroupsAsync()
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync("ToolGroup");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error getting tool groups: {response.StatusCode}");
                    return new List<ToolGroup>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to get tool groups: {error}");
                    return new List<ToolGroup>();
                }

                var toolGroups = new List<ToolGroup>();
                foreach (var item in apiResponse.Result)
                {
                    toolGroups.Add(new ToolGroup
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Name = item.GetProperty("name").GetString(),
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        IsPremium = item.GetProperty("isPremium").GetBoolean(),
                        IsEnabled = item.GetProperty("isEnabled").GetBoolean(),
                        IsFavorite = item.GetProperty("isFavorite").GetBoolean()
                    });
                }

                return toolGroups;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error getting tool groups: {ex.Message}");
                return new List<ToolGroup>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing tool groups response: {ex.Message}");
                return new List<ToolGroup>();
            }
        }

        public async Task<List<Tool>> GetToolsByGroupIdAsync(int groupId)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.GetAsync($"ToolGroup/{groupId}/todoItems");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error getting tools by group id: {response.StatusCode}");
                    return new List<Tool>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to get tools by group: {error}");
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
                Debug.WriteLine($"Error getting tools by group ID: {ex.Message}");
                return new List<Tool>();
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing tool groups response: {ex.Message}");
                return new List<Tool>();
            }
        }

        // Admin methods
        public async Task<int> AddToolGroupAsync(string name, string description = null)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var groupData = new { name, description };
                var response = await _httpClient.PostAsJsonAsync("ToolGroup/add", groupData);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error adding tool group: {response.StatusCode}");
                    return -1;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Failed to add tool group: {error}");
                    return -1;
                }

                return apiResponse.Result.GetProperty("id").GetInt32();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error adding tool group: {ex.Message}");
                return -1;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing tool groups response: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateToolGroupAsync(int id, string name, string description = null)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var groupData = new { id, name, description };
                var response = await _httpClient.PutAsJsonAsync($"ToolGroup/update/{id}", groupData);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error updating tool group: {response.StatusCode}");
                    return false;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error updating tool group: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteToolGroupAsync(int id)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await _httpClient.DeleteAsync($"ToolGroup/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"HTTP error deleting tool group: {response.StatusCode}");
                    return false;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error deleting tool group: {ex.Message}");
                return false;
            }
        }
    }
}
