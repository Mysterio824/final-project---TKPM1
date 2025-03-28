using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using DevTools.Data;
using DevTools.Middleware;
using DevTools.Interfaces.Services;
using DevTools.Services;
using DevTools.Interfaces.Repositories;
using DevTools.Repositories;
using DevTools.Strategies;
using DevTools.Strategies.ToolStrategies;
using DevTools.Interfaces.Core;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DevTools;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use APP_PORT from environment
        ConfigureKestrel(builder);

        // Logging
        ConfigureLogging(builder);

        // Add services
        ConfigureServices(builder);

        var app = builder.Build();

        // Initialize database schema
        await InitializeDatabaseSchemaAsync(app);

        try
        {
            // Configure Tool Watcher
            ConfigureToolWatcher(app);

            // Configure middleware and routing
            ConfigureMiddleware(app);

            app.Logger.LogInformation("Application fully started. Listening on {Urls}",
                string.Join(", ", app.Urls.Count != 0 ? app.Urls : [$"http://0.0.0.0:{builder.Configuration["APP_PORT"] ?? "5000"}"]));

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Application failed to start. Details: {Message}", ex.Message);
            throw;
        }
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        var appPort = Environment.GetEnvironmentVariable("APP_PORT") ?? "5000";
        builder.WebHost.ConfigureKestrel(options =>
        {
            if (!int.TryParse(appPort, out var port) || port < 1 || port > 65535)
            {
                throw new InvalidOperationException($"Invalid APP_PORT value: {appPort}. Must be a valid port number between 1 and 65535.");
            }
            options.ListenAnyIP(port);
        });
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger configuration
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DevTools API",
                Version = "v1",
                Description = "API for managing DevTools application."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });

        // JWT Authentication
        ConfigureJwtAuthentication(builder);

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IToolRepository, ToolRepository>();
        builder.Services.AddScoped<IFavoriteToolRepository, FavoriteToolRepository>();

        // Redis Configuration
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "devtools-redis-1:6379"));

        // Services
        builder.Services.AddScoped<IRedisService, RedisService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IToolService, ToolService>();
        builder.Services.AddScoped<IAccountService, AccountService>();

        // Tool Actions
        builder.Services.AddScoped<IToolActionStrategy, SetPremiumToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, SetFreeToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, DisableToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, EnableToolStrategy>();

        builder.Services.AddScoped<ToolActionStrategyFactory>();

        // Database Initializer
        builder.Services.AddScoped<DatabaseInitializer>();

        // DbContext Configuration
        ConfigureDbContext(builder);
    }

    private static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
        builder.Services.AddAuthorization();
    }

    private static void ConfigureDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")
            )
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

            if (builder.Environment.IsDevelopment())
            {
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        });
    }

    private static async Task InitializeDatabaseSchemaAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await databaseInitializer.InitializeDatabaseSchemaAsync();
    }

    private static void ConfigureToolWatcher(WebApplication app)
    {
        var toolService = app.Services.CreateScope().ServiceProvider.GetRequiredService<IToolService>();
        var toolWatcher = new ToolWatcher("Tools", toolService);
        toolWatcher.StartWatching();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
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
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}