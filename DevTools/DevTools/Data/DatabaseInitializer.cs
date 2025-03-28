using DevTools.Entities;
using DevTools.Enums;
using DevTools.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DevTools.Data
{
    public class DatabaseInitializer(
        ApplicationDbContext context,
        ILogger<DatabaseInitializer> logger,
        IUserRepository userRepository)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<DatabaseInitializer> _logger = logger;
        private readonly IUserRepository _userRepository = userRepository;

        public async Task InitializeDatabaseSchemaAsync()
        {
            try
            {
                _logger.LogInformation("Dynamically initializing database schema...");
                await CreateDatabaseSchemaAsync();
                _logger.LogInformation("Database schema initialized successfully.");

                await EnsureDefaultAdminExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database schema.");
                throw;
            }
        }

        private async Task CreateDatabaseSchemaAsync()
        {
            var model = _context.Model;
            var sqlBuilder = new StringBuilder();

            sqlBuilder.AppendLine("CREATE SCHEMA IF NOT EXISTS public;");

            // Iterate over all entity types in the DbContext
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName))
                    continue;

                sqlBuilder.AppendLine($"CREATE TABLE IF NOT EXISTS public.\"{tableName}\" (");

                // Columns
                var columns = new List<string>();
                var storeObject = StoreObjectIdentifier.Table(tableName, "public");

                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName(storeObject);
                    var columnType = GetPostgresType(property);
                    var isNullable = property.IsNullable ? "NULL" : "NOT NULL";

                    var columnDefinition = $"\"{columnName}\" {columnType} {isNullable}";
                    columns.Add(columnDefinition);
                }

                sqlBuilder.AppendLine(string.Join(",\n", columns));

                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var pkColumns = string.Join(", ", primaryKey.Properties.Select(p => $"\"{p.GetColumnName(storeObject)}\""));
                    sqlBuilder.AppendLine($", PRIMARY KEY ({pkColumns})");
                }

                sqlBuilder.AppendLine(");");
            }

            var sql = sqlBuilder.ToString();
            _logger.LogDebug("Generated SQL:\n{0}", sql);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute SQL: {Sql}", sql);
                throw;
            }
        }

        private async Task EnsureDefaultAdminExistsAsync()
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
                    Role = UserRole.Admin,
                    IsPremium = true
                };

                await _userRepository.AddAsync(adminUser);
                _logger.LogInformation("Default admin account created successfully.");
            }
            else
            {
                _logger.LogInformation("Default admin already exists.");
            }
        }

        private static string GetPostgresType(IProperty property)
        {
            var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

            if (clrType.IsEnum) return "INTEGER";

            string baseType = clrType switch
            {
                Type t when t == typeof(int) => "INTEGER",
                Type t when t == typeof(long) => "BIGINT",
                Type t when t == typeof(string) => $"VARCHAR({property.GetMaxLength() ?? 255})",
                Type t when t == typeof(bool) => "BOOLEAN",
                Type t when t == typeof(DateTime) => "TIMESTAMP WITH TIME ZONE",
                Type t when t == typeof(decimal) => "NUMERIC",
                Type t when t == typeof(Guid) => "UUID",
                _ => throw new NotSupportedException($"Type {clrType.Name} is not supported for PostgreSQL mapping.")
            };

            if (property.IsPrimaryKey() && clrType == typeof(int) && ((IEntityType)property.DeclaringType).FindPrimaryKey()?.Properties.Count == 1)
            {
                return "INTEGER GENERATED ALWAYS AS IDENTITY";
            }

            return baseType;
        }
    }
}
