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
using System.Diagnostics;
using DevTools.Strategies; // For Process

internal class Program
{
    private static async Task Main(string[] args)
    {
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

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IToolRepository, ToolRepository>();
        builder.Services.AddScoped<IFavoriteToolRepository, FavoriteToolRepository>();

        // Services
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "devtools-redis-1:6379"));

        builder.Services.AddScoped<IRedisService, RedisService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IToolService, ToolService>();

        builder.Services.AddScoped<ToolActionStrategyFactory>();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        try
        {
            var toolService = app.Services.CreateScope().ServiceProvider.GetRequiredService<IToolService>();
            var toolWatcher = new ToolWatcher("Tools", toolService);
            toolWatcher.StartWatching();

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

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Application failed to start. Details: {Message}", ex.Message);
            throw;
        }
    }
}