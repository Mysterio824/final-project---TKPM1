using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.DataAccess.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DatabaseContext>();
        var initializer = services.GetRequiredService<DatabaseInitializer>();

        if (context.Database.IsNpgsql())
        {
            await context.Database.MigrateAsync();
        }

        await initializer.InitializeAsync();
    }
}