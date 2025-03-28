using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using DevTools.Infrastructure.Data;
using DevTools.Application.Interfaces.Repositories;
using DevTools.Application.Interfaces.Services;
using DevTools.Infrastructure.Repositories;
using DevTools.Infrastructure.Services;
using DevTools.Infrastructure.Strategies;
using DevTools.Infrastructure.Strategies.ToolStrategies;
using DevTools.Application.Interfaces.Core;

namespace DevTools.API.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IToolRepository, ToolRepository>();
            services.AddScoped<IFavoriteToolRepository, FavoriteToolRepository>();

            // Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IFavoriteToolService, FavoriteToolService>();
            services.AddScoped<IPremiumService, PremiumService>();

            services.AddScoped<IToolCommandService, ToolCommandService>();
            services.AddScoped<IToolExecutionService, ToolExecutionService>();
            services.AddScoped<IToolQueryService, ToolQueryService>();
            services.AddScoped<IFileService, FileService>();


            // Tool Actions Strategies
            services.AddScoped<IToolActionStrategy, SetPremiumToolStrategy>();
            services.AddScoped<IToolActionStrategy, SetFreeToolStrategy>();
            services.AddScoped<IToolActionStrategy, DisableToolStrategy>();
            services.AddScoped<IToolActionStrategy, EnableToolStrategy>();

            services.AddScoped<ToolActionStrategyFactory>();

            // Database Initializer
            services.AddScoped<DatabaseInitializer>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            services.AddAuthorization();
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")
                )
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();

                if (environment.IsDevelopment())
                {
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                }
            });

            return services;
        }

        public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"] ?? "devtools-redis-1:6379"));
            return services;
        }
    }
}
