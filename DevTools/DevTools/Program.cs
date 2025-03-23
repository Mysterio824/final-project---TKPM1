// DevTools/Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevTools.Data;
using DevTools.Middleware;
using DevTools.Interfaces.Services;
using DevTools.Services;
using DevTools.Interfaces.Repositories;
using DevTools.Repositories;
using StackExchange.Redis;
using System.Diagnostics; // For Process
using Microsoft.Extensions.Hosting; // For IHost

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
builder.Services.AddScoped<IRedisService, RedisService>();

var app = builder.Build();

// Start Redis server
Process redisProcess = null;
try
{
    app.Logger.LogInformation("Starting Redis server...");
    redisProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = @"C:\Program Files\Redis\redis-server.exe",
            Arguments = "--port 6379", // Ensure it matches your config
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    redisProcess.Start();
    app.Logger.LogInformation("Redis server started with PID: {PID}", redisProcess.Id);

    // Log Redis output
    _ = Task.Run(async () =>
    {
        while (!redisProcess.StandardOutput.EndOfStream)
        {
            var line = await redisProcess.StandardOutput.ReadLineAsync();
            app.Logger.LogInformation("Redis: {Output}", line);
        }
    });
    _ = Task.Run(async () =>
    {
        while (!redisProcess.StandardError.EndOfStream)
        {
            var line = await redisProcess.StandardError.ReadLineAsync();
            app.Logger.LogError("Redis Error: {Error}", line);
        }
    });
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to start Redis server");
    throw;
}

// Hook into application shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (redisProcess != null && !redisProcess.HasExited)
    {
        app.Logger.LogInformation("Stopping Redis server...");
        redisProcess.Kill(); // Forceful stop; use redis-cli shutdown for graceful stop if needed
        redisProcess.WaitForExit();
        app.Logger.LogInformation("Redis server stopped");
    }
});

try
{
    app.Logger.LogInformation("Application starting on {Urls}",
        builder.Configuration["applicationUrl"] ?? "http://localhost:5000");

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevTools API V1"));
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<JwtMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();


    app.Logger.LogInformation("Application fully started. Listening on {Urls}",
        builder.Configuration["applicationUrl"] ?? "http://localhost:5000");

    await app.RunAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Application failed to start. Details: {Message}", ex.Message);
    throw;
}
finally
{
    // Ensure Redis stops even on unhandled exceptions
    if (redisProcess != null && !redisProcess.HasExited)
    {
        redisProcess.Kill();
        redisProcess.WaitForExit();
    }
}