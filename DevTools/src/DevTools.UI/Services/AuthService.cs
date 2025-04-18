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
        private readonly IMockDao _mockDao;

        public AuthService(IMockDao mockDao)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://localhost:5000");
            _mockDao = mockDao;
        }
        public event Action? OnLoginStatusChanged;
        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password })
                ?? throw new Exception("Failed to login.");
            if (response.IsSuccessStatusCode)
            {
                var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<User>>();
                if (apiResult?.Succeeded == true)
                {
                    JwtTokenManager.SaveToken(apiResult.Result!.Token);
                    AppServices.ApiService.UpdateAuthorizationHeader();
                    return true;
                }
            } else
            {

            }
            //var response = await _mockDao.PostAsync<ApiResult<User>>("api/auth/login", new { username, password });
            //if (response?.Succeeded == true)
            //{
            //    JwtTokenManager.SaveToken(response.Result!.Token);
            //    AppServices.ApiService.UpdateAuthorizationHeader();
            //    OnLoginStatusChanged?.Invoke();
            //    return true;
            //}
            return false;
        }

        public void Logout()
        {
            JwtTokenManager.ClearToken();
            //AppServices.ApiService.UpdateAuthorizationHeader();
            OnLoginStatusChanged?.Invoke();
        }
    }
}
