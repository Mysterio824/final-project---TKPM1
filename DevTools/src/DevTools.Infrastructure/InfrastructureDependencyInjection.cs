using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevTools.Infrastructure.Persistence;
using DevTools.Infrastructure.Repositories;
using DevTools.Infrastructure.Repositories.impl;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DevTools.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDatabase(configuration, environment);
        services.AddRepositories();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DatabaseContextSeed>();

        return services;
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<IToolGroupRepository, ToolGroupRepository>();
        services.AddScoped<IFavoriteToolRepository, FavoriteToolRepository>();
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")
            )
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

            if (environment.IsDevelopment())
            {
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        });

        return services;
    }
}