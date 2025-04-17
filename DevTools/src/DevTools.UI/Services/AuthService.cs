using DevTools.UI.Models;
using DevTools.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                if (apiResult?.Succeeded == true)
                {
                    JwtTokenManager.SaveToken(apiResult.Result!.Token);
                    AppServices.ApiService.UpdateAuthorizationHeader();
                    return true;
                }
            }
            return false;
        }

        public void Logout()
        {
            JwtTokenManager.ClearToken();
            AppServices.ApiService.UpdateAuthorizationHeader();
        }
    }
}
