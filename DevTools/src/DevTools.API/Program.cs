using DevTools.API.Configurations;
using DevTools.API.Middleware;
using DevTools.Application.Services;
using DevTools.Infrastructure;
using DevTools.Application;
using DevTools.Infrastructure.Persistence;

namespace DevTools.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        KestrelConfig.ConfigureKestrel(builder);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Register Services
        builder.Services.AddDataAccess(builder.Configuration, builder.Environment);
        builder.Services.AddApplication();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddSwaggerServices();
        builder.Services.AddRedisConfiguration(builder.Configuration);
        builder.Services.AddEmailConfiguration(builder.Configuration);
        builder.Services.AddLinkGeneratorConfiguration(builder.Configuration);

        var app = builder.Build();

        using var scope = app.Services.CreateScope();

        await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

        try
        {
            ConfigureToolWatcher(app);
            ConfigureMiddleware(app);

            app.Logger.LogInformation("Application fully started. Listening on {Urls}",
                string.Join(", ", app.Urls.Count != 0 ? app.Urls : [$"http://localhost:{builder.Configuration["APP_PORT"] ?? "5000"}"]));

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Application failed to start. Details: {Message}", ex.Message);
            throw;
        }
    }

    private static void ConfigureToolWatcher(WebApplication app)
    {
        var toolService = app.Services.CreateScope().ServiceProvider.GetRequiredService<IToolCommandService>();
        var toolWatcher = new ToolWatcher("Tools", toolService);
        toolWatcher.StartWatching();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevTools API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<JwtMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
