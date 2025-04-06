using DevTools.Application.Services.Impl;
using DevTools.Application.Services;
using DevTools.Application.Strategies.Core;
using DevTools.Application.Strategies.ToolStrategies;
using DevTools.Application.Strategies;
using DevTools.Infrastructure.Strategies.ToolStrategies;
using Microsoft.Extensions.DependencyInjection;
using DevTools.Application.Common.Email;
using Microsoft.Extensions.Configuration;
using DevTools.Application.Common.LinkGenerator;
using DevTools.Application.MappingProfiles;
using Microsoft.AspNetCore.Hosting;
namespace DevTools.Application;

public static class ApplicationDependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();

        services.RegisterAutoMapper();

        services.AddStrategies();

        return services;
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ILinkGeneratorService, LinkGeneratorService>();
        services.AddScoped<ITemplateService, TemplateService>();

        services.AddScoped<IFavoriteToolService, FavoriteToolService>();
        services.AddScoped<IPremiumService, PremiumService>();

        services.AddScoped<IToolCommandService, ToolCommandService>();
        services.AddScoped<IToolQueryService, ToolQueryService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IToolGroupService, ToolGroupService>();
    }

    public static void AddStrategies(this IServiceCollection services)
    {
        services.AddScoped<IToolActionStrategy, SetPremiumToolStrategy>();
        services.AddScoped<IToolActionStrategy, SetFreeToolStrategy>();
        services.AddScoped<IToolActionStrategy, DisableToolStrategy>();
        services.AddScoped<IToolActionStrategy, EnableToolStrategy>();
        services.AddScoped<ToolActionStrategyFactory>();
    }

    private static void RegisterAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(IMappingProfilesMarker));
    }

    public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection("EmailSettings").Get<SmtpSettings>());
    }

    public static void AddLinkGeneratorConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new LinkGenerateSettings
        {
            ApplicationUrl = configuration.GetValue<string>("applicationUrl")
        };

        if (string.IsNullOrEmpty(settings.ApplicationUrl))
        {
            throw new InvalidOperationException("ApplicationUrl is not configured");
        }

        services.AddSingleton(settings);
    }
}
