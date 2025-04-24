using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ApiServiceFactory
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiServiceFactory(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
        }

        public AuthService CreateAuthService()
        {
            return new AuthService(_httpClient, _baseUrl);
        }

        public AccountService CreateAccountService()
        {
            return new AccountService(_httpClient, _baseUrl);
        }

        public ToolService CreateToolService()
        {
            return new ToolService(_httpClient, _baseUrl);
        }

        public ToolGroupService CreateToolGroupService()
        {
            return new ToolGroupService(_httpClient, _baseUrl);
        }

        // Set authentication token across all services
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
    }
}
