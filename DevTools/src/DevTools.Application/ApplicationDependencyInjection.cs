using DevTools.Application.Services.Impl;
using DevTools.Application.Services;
using DevTools.Application.Strategies.Core;
using DevTools.Application.Strategies.ToolStrategies;
using DevTools.Application.Strategies;
using DevTools.Infrastructure.Strategies.ToolStrategies;
using Microsoft.Extensions.DependencyInjection;
namespace DevTools.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
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

        return services;
    }
}
