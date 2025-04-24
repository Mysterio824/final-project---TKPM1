using DevTools.UI.Models;
using DevTools.UI.Utils;
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
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthService(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var credentials = new { email, password };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Auth/login", credentials);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Login failed: {string.Join(", ", apiResponse.Errors)}");
                    return null;
                }

                var loginResponse = apiResponse.Result;

                var user = new User
                {
                    Id = loginResponse.GetProperty("id").ToString(),
                    Name = loginResponse.GetProperty("username").ToString(),
                    Email = loginResponse.GetProperty("email").ToString(),
                    Token = loginResponse.GetProperty("token").ToString(),
                    RefreshToken = loginResponse.GetProperty("refreshToken").ToString(),
                    Role = loginResponse.GetProperty("role").GetInt32()
                };

                return user;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during login: {ex.Message}");
                return null;
            }
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registrationData = new { username, email, password };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Auth/register", registrationData);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Registration failed: {string.Join(", ", apiResponse.Errors)}");
                    return null;
                }

                var registerResponse = apiResponse.Result;

                var user = new User
                {
                    Id = registerResponse.GetProperty("id").ToString(),
                    Name = registerResponse.GetProperty("username").ToString(),
                    Email = registerResponse.GetProperty("email").ToString(),
                    Token = registerResponse.GetProperty("token").ToString(),
                    RefreshToken = registerResponse.GetProperty("refreshToken").ToString(),
                    Role = registerResponse.GetProperty("role").GetInt32()
                };

                return user;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during registration: {ex.Message}");
                return null;
            }
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenRequest = new { refreshToken };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Auth/refresh-token", tokenRequest);

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

                if (!apiResponse.Succeeded)
                {
                    Debug.WriteLine($"Token refresh failed: {string.Join(", ", apiResponse.Errors)}");
                    return (null, null);
                }

                var tokenResponse = apiResponse.Result;

                return (
                    tokenResponse.GetProperty("accessToken").GetString(),
                    tokenResponse.GetProperty("refreshToken").GetString()
                );
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during token refresh: {ex.Message}");
                return (null, null);
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Auth/logout", null);

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during logout: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Auth/verify-email?token={token}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during email verification: {ex.Message}");
                return false;
            }
        }
    }
}
