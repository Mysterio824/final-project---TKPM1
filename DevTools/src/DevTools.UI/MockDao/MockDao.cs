using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DevTools.UI.Models;

public class MockDao : IMockDao
{
    private readonly Dictionary<string, object> _mockData = new();
    private readonly Dictionary<string, (string Password, User User)> _mockUsers = new();

    public MockDao()
    {
        _mockUsers["admin"] = ("admin123", new User
        {
            Username = "admin",
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VybmFtZSI6ImFkbWluIiwicm9sZSI6ImFkbWluIiwiaXNfcHJlbWl1bSI6InRydWUiLCJleHAiOjE3MDAwMDAwMDB9.abc123def456ghi789jkl0mnopqrstu",
            IsAdmin = true,
            IsPremium = true
        });

        _mockUsers["premium"] = ("premium123", new User
        {
            Username = "premium",
            Token = "premium-token",
            IsAdmin = false,
            IsPremium = true
        });

        _mockUsers["user"] = ("user123", new User
        {
            Username = "user",
            Token = "user-token",
            IsAdmin = false,
            IsPremium = false
        });

        // Add mock data for ToolService
        AddMockResponse("tools/all", new ApiResult<List<Tool>>
        {
            Succeeded = true,
            Result = new List<Tool>
            {
                new Tool { Id = 1, Name = "Tool 1", Description = "Description 1", IsEnabled = true, IsPremium = false, DllPath = "path1" },
                new Tool { Id = 2, Name = "Tool 2", Description = "Description 2", IsEnabled = false, IsPremium = true, DllPath = "path2" }
            }
        });

        AddMockResponse("tools/favorites", new ApiResult<List<Tool>>
        {
            Succeeded = true,
            Result = new List<Tool>
            {
                new Tool { Id = 1, Name = "Tool 1", Description = "Description 1", IsEnabled = true, IsPremium = false, DllPath = "path1" }
            }
        });

        AddMockResponse("tools/1", new ApiResult<Tool>
        {
            Succeeded = true,
            Result = new Tool { Id = 1, Name = "Tool 1", Description = "Description 1", IsEnabled = true, IsPremium = false, DllPath = "path1" }
        });

        AddMockResponse("admin/tools/1/set-enabled", new ApiResult<bool>
        {
            Succeeded = true,
            Result = true
        });
        AddMockResponse("admin/tools/upload", new ApiResult<bool>
        {
            Succeeded = true,
            Result = true
        });
        //AddMockResponse("auth/login", new ApiResult<User>
        //{
        //    Succeeded = true,
        //    Result = new User
        //    {
        //        Username = "testuser",
        //        Token = "mock-jwt-token",
        //        IsAdmin = true,
        //        IsPremium = true
        //    }
        //});
        AddMockResponse("tools/1/download", new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }); // Mock DLL content
        AddMockResponse("tools/2/download", new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }); // Mock DLL content

    }

    public void AddMockResponse<T>(string endpoint, T response)
    {
        _mockData[endpoint] = response!;
    }

    public Task<T?> GetAsync<T>(string endpoint)
    {
        if (_mockData.TryGetValue(endpoint, out var data))
        {
            return Task.FromResult((T?)data);
        }
        return Task.FromResult(default(T));
    }

    public Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        if (endpoint == "auth/login" && data is not null)
        {
            var usernameProp = data.GetType().GetProperty("username");
            var passwordProp = data.GetType().GetProperty("password");

            var username = usernameProp?.GetValue(data)?.ToString();
            var password = passwordProp?.GetValue(data)?.ToString();

            if (username != null && password != null && _mockUsers.TryGetValue(username, out var entry))
            {
                if (entry.Password == password)
                {
                    Debug.WriteLine(entry.User.IsPremium);
                    var result = new ApiResult<User>
                    {
                        Succeeded = true,
                        Result = entry.User
                    };
                    return Task.FromResult((T?)(object)result);
                }
            }

            // Failed login
            var fail = new ApiResult<User>
            {
                Succeeded = false,
                Errors = new List<string> { "Invalid username or password" }
            };
            return Task.FromResult((T?)(object)fail);
        }
        if (_mockData.TryGetValue(endpoint, out var response))
        {
            return Task.FromResult((T?)response);
        }
        return Task.FromResult(default(T));
    }
}
