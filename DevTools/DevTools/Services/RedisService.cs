using StackExchange.Redis;
using System.Text.Json;
using DevTools.Interfaces.Services;
using DevTools.DTOs.Request;

namespace DevTools.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _redisDb;
    private readonly string _instanceName;

    public RedisService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redisDb = redis.GetDatabase();
        _instanceName = configuration["Redis:InstanceName"] ?? "DevTools";
    }

    public async Task StoreUnverifiedUserAsync(string email, RegisterDto registerDto, TimeSpan expiration)
    {
        var key = $"{_instanceName}:unverified:user:{email}";
        var json = JsonSerializer.Serialize(registerDto);
        await _redisDb.StringSetAsync(key, json, expiration);
    }

    public async Task<RegisterDto?> GetUnverifiedUserAsync(string email)
    {
        var key = $"{_instanceName}:unverified:user:{email}";
        var json = await _redisDb.StringGetAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<RegisterDto>(json.ToString()) : null;
    }

    public async Task RemoveUnverifiedUserAsync(string email)
    {
        var key = $"{_instanceName}:unverified:user:{email}";
        await _redisDb.KeyDeleteAsync(key);
    }

    public async Task StoreVerificationTokenAsync(string email, string token, TimeSpan expiration)
    {
        var key = $"{_instanceName}:verification:token:{email}";
        await _redisDb.StringSetAsync(key, token, expiration);
    }

    public async Task<string?> GetVerificationTokenAsync(string email)
    {
        var key = $"{_instanceName}:verification:token:{email}";
        var token = await _redisDb.StringGetAsync(key);
        return token.HasValue ? token.ToString() : null;
    }

    public async Task RemoveVerificationTokenAsync(string email)
    {
        var key = $"{_instanceName}:verification:token:{email}";
        await _redisDb.KeyDeleteAsync(key);
    }

    public async Task StoreRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration)
    {
        var key = $"{_instanceName}:refresh:token:{userId}";
        await _redisDb.StringSetAsync(key, refreshToken, expiration);
    }

    public async Task<string?> GetRefreshTokenAsync(string userId)
    {
        var key = $"{_instanceName}:refresh:token:{userId}";
        var token = await _redisDb.StringGetAsync(key);
        return token.HasValue ? token.ToString() : null;
    }

    public async Task RemoveRefreshTokenAsync(string userId)
    {
        var key = $"{_instanceName}:refresh:token:{userId}";
        await _redisDb.KeyDeleteAsync(key);
    }

    public async Task BlacklistAccessTokenAsync(string token, TimeSpan expiration)
    {
        var key = $"{_instanceName}:blacklist:token:{token}";
        await _redisDb.StringSetAsync(key, "blacklisted", expiration);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        var key = $"{_instanceName}:blacklist:token:{token}";
        var value = await _redisDb.StringGetAsync(key);
        return value.HasValue;
    }

    public async Task<string?> GetEmailByVerificationTokenAsync(string token)
    {
        var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
        var pattern = $"{_instanceName}:verification:token:*";
        foreach (var key in server.Keys(pattern: pattern))
        {
            var storedToken = await _redisDb.StringGetAsync(key);
            if (storedToken == token)
            {
                return key.ToString().Replace($"{_instanceName}:verification:token:", "");
            }
        }
        return null;
    }
}