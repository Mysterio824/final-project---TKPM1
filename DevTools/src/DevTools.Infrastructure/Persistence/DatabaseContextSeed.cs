using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using DevTools.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace DevTools.Infrastructure.Persistence;

public class DatabaseContextSeed(
    ILogger<DatabaseContextSeed> logger, 
    IUserRepository userRepository
) {
    private readonly ILogger<DatabaseContextSeed> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task SeedDatabaseAsync(DatabaseContext context)
    {
        try
        {
            const string adminEmail = "admin@devtools.com";
            const string adminUsername = "admin";
            const string adminPassword = "Admin@123";

            var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                _logger.LogInformation("Creating default admin account...");

                var adminUser = new User
                {
                    Username = adminUsername,
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = UserRole.Admin
                };

                await _userRepository.AddAsync(adminUser);
                _logger.LogInformation("Default admin account created successfully.");
            }
            else
            {
                _logger.LogInformation("Default admin already exists.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}