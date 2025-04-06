using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DevTools.Infrastructure.Persistence;
public class DatabaseInitializer(
    DatabaseContext context, 
    ILogger<DatabaseInitializer> logger, 
    DatabaseContextSeed seed)
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger<DatabaseInitializer> _logger = logger;
    private readonly DatabaseContextSeed _seed = seed;
    
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");
            
            try
            {
                await _seed.SeedDatabaseAsync(_context);
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (DevTools.Domain.Exceptions.ResourceNotFoundException ex)
            {
                _logger.LogWarning(ex, "Skipping some seed operations due to missing table or schema. This may be normal during first run.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding. Application will continue but some data may be missing.");
            }
            
            _logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during database initialization.");
            throw;
        }
    }
}