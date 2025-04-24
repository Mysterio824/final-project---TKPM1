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
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ToolGroupService(HttpClient httpClient, string baseUrl)
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

        public async Task<List<ToolGroup>> GetAllToolGroupsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/ToolGroup");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<ToolGroup>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Failed to get tool groups: {string.Join(", ", apiResponse.Errors)}");
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
        }

        public async Task<List<Tool>> GetToolsByGroupIdAsync(int groupId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/ToolGroup/{groupId}/todoItems");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<Tool>();
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<JsonElement>>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Failed to get tools by group: {string.Join(", ", apiResponse.Errors)}");
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
        }

        // Admin methods
        public async Task<int> AddToolGroupAsync(string name, string description = null)
        {
            try
            {
                var groupData = new { name, description };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/ToolGroup/add", groupData);

                if (!response.IsSuccessStatusCode)
                {
                    return -1;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Adding tool group failed: {string.Join(", ", apiResponse.Errors)}");
                    return -1;
                }

                return apiResponse.Result.GetProperty("id").GetInt32();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error adding tool group: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateToolGroupAsync(int id, string name, string description = null)
        {
            try
            {
                var groupData = new { id, name, description };
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/ToolGroup/update/{id}", groupData);

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
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/ToolGroup/{id}");
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
