using Microsoft.Extensions.Logging;

namespace DevTools.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly DatabaseContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly DatabaseContextSeed _seed;

    public DatabaseInitializer(DatabaseContext context, ILogger<DatabaseInitializer> logger, DatabaseContextSeed seed)
    {
        _context = context;
        _logger = logger;
        _seed = seed;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");
            await _context.Database.EnsureCreatedAsync();
            await _seed.SeedDatabaseAsync(_context);
            _logger.LogInformation("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}