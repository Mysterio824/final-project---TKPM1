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
using Windows.Media.Protection.PlayReady;

namespace DevTools.UI.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("Login failed: Email or password is empty.");
                return null;
            }

            var _httpClient = _httpClientFactory.CreateClient("UnauthenticatedApiClient");
            try
            {
                var credentials = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("Auth/login", credentials);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Login HTTP error: {response.StatusCode}");
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Login failed: {error}");
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
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing login response: {ex.Message}");
                return null;
            }
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("Register failed: Email or password is empty.");
                return null;
            }
            var _httpClient = _httpClientFactory.CreateClient("UnauthenticatedApiClient");
            try
            {
                var registrationData = new { username, email, password };
                var response = await _httpClient.PostAsJsonAsync("Auth/register", registrationData);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Register HTTP error: {response.StatusCode}");
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Registration failed: {error}");
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
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing register response: {ex.Message}");
                return null;
            }
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var tokenRequest = new { refreshToken };
                var response = await _httpClient.PostAsJsonAsync("Auth/refresh-token", tokenRequest);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Refresh token HTTP error: {response.StatusCode}");
                    return (null, null);
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();
                if (!apiResponse.Succeeded)
                {
                    var error = string.Join(", ", apiResponse.Errors);
                    Debug.WriteLine($"Token refresh failed: {error}");
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
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing token refresh response: {ex.Message}");
                return (null, null);
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            var _httpClient = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync("Auth/logout", null);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Logout HTTP error: {response.StatusCode}");
                    return false;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during logout: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing logout response: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var _httpClient = _httpClientFactory.CreateClient("UnauthenticatedApiClient");
            try
            {
                var response = await _httpClient.GetAsync($"Auth/verify-email?token={token}");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Verify email HTTP error: {response.StatusCode}");
                    return false;
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error during email verification: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error parsing verify email response: {ex.Message}");
                return false;
            }
        }
    }
}
