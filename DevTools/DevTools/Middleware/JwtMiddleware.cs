using DevTools.Enums;
using DevTools.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevTools.Middleware;

public class JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<JwtMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IRedisService redisService)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            if (await redisService.IsTokenBlacklistedAsync(token))
            {
                _logger.LogWarning("Blacklisted token used in request to {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Access in blacklist");
                return;
            }

            if (!AttachUserToContext(context, token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        else
        {
            SetAnonymousUser(context);
            _logger.LogInformation("No Authorization token provided in request to {Path}", context.Request.Path);
        }

        await _next(context);
    }

    private bool AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key is not configured."));

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value 
                        ?? throw new SecurityTokenException("Invalid JWT token");
            var roleClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value
                     ?? throw new SecurityTokenException("Missing role");

            var claims = new List<Claim> { 
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value),
                new(ClaimTypes.Role, roleClaim),
            };

            var identity = new ClaimsIdentity(claims, roleClaim);
            context.User = new ClaimsPrincipal(identity);
            
            _logger.LogInformation("User {UserId} authenticated via JWT for request {Path}", userId, context.Request.Path);

            return true;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "JWT validation failed for token: {Token}", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating JWT for token: {Token}", token);
        }
        return false;
    }

    private static void SetAnonymousUser(HttpContext context)
    {
        var anonymousIdentity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "-1"),
            new Claim(ClaimTypes.Name, "Anonymous"),
            new Claim(ClaimTypes.Role, UserRole.Anonymous.ToString()),
        ]);

        context.User = new ClaimsPrincipal(anonymousIdentity);
    }
}