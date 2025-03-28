using DevTools.API.Configuration;
using DevTools.API.Configurations;
using DevTools.API.Middleware;
using DevTools.Application.Interfaces.Services;
using DevTools.Infrastructure.Data;

namespace DevTools;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        KestrelConfig.ConfigureKestrel(builder);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Register Services
        builder.Services.AddApplicationServices();
        builder.Services.AddDatabase(builder.Configuration, builder.Environment);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddSwaggerServices();
        builder.Services.AddRedisConfiguration(builder.Configuration);

        var app = builder.Build();

        await InitializeDatabaseSchemaAsync(app);

        try
        {
            ConfigureToolWatcher(app);
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

    private static async Task InitializeDatabaseSchemaAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await databaseInitializer.InitializeDatabaseSchemaAsync();
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
            app.UseSwaggerUIConfig();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<JwtMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
