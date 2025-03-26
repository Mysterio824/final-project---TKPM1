using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Text;
using DevTools.Data;
using DevTools.Middleware;
using DevTools.Interfaces.Services;
using DevTools.Services;
using DevTools.Interfaces.Repositories;
using DevTools.Repositories;
using DevTools.Strategies;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using DevTools.Entities;
using DevTools.Enums;
using DevTools.Strategies.ToolStrategy;

namespace DevTools;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use APP_PORT from environment
        ConfigureKestrel(builder);

        // Logging
        ConfigureLogging(builder);

        // Add services
        ConfigureServices(builder);

        var app = builder.Build();

        // Initialize database schema
        await InitializeDatabaseSchemaAsync(app);

        try
        {
            // Configure Tool Watcher
            ConfigureToolWatcher(app);

            // Configure middleware and routing
            ConfigureMiddleware(app);

            app.Logger.LogInformation("Application fully started. Listening on {Urls}",
                string.Join(", ", app.Urls.Any() ? app.Urls : new[] { $"http://0.0.0.0:{builder.Configuration["APP_PORT"] ?? "5000"}" }));

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Application failed to start. Details: {Message}", ex.Message);
            throw;
        }
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        var appPort = Environment.GetEnvironmentVariable("APP_PORT") ?? "5000";
        builder.WebHost.ConfigureKestrel(options =>
        {
            if (!int.TryParse(appPort, out var port) || port < 1 || port > 65535)
            {
                throw new InvalidOperationException($"Invalid APP_PORT value: {appPort}. Must be a valid port number between 1 and 65535.");
            }
            options.ListenAnyIP(port);
        });
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger configuration
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DevTools API", Version = "v1" });
        });

        // JWT Authentication
        ConfigureJwtAuthentication(builder);

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IToolRepository, ToolRepository>();
        builder.Services.AddScoped<IFavoriteToolRepository, FavoriteToolRepository>();

        // Redis Configuration
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "devtools-redis-1:6379"));

        // Services
        builder.Services.AddScoped<IRedisService, RedisService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IToolService, ToolService>();
        builder.Services.AddScoped<IAccountService, AccountService>();

        // Tool Actions
        builder.Services.AddScoped<IToolActionStrategy, SetPremiumToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, SetFreeToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, DisableToolStrategy>();
        builder.Services.AddScoped<IToolActionStrategy, EnableToolStrategy>();

        builder.Services.AddScoped<ToolActionStrategyFactory>();

        // DbContext Configuration
        ConfigureDbContext(builder);
    }

    private static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
        builder.Services.AddAuthorization();
    }

    private static void ConfigureDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")
            )
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

            if (builder.Environment.IsDevelopment())
            {
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        });
    }

    private static async Task InitializeDatabaseSchemaAsync(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Dynamically initializing database schema...");
                await CreateDatabaseSchemaAsync(context, logger);
                logger.LogInformation("Database schema initialized successfully.");

                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                await EnsureDefaultAdminExistsAsync(userRepository, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database schema.");
                throw;
            }
        }
    }

    private static void ConfigureToolWatcher(WebApplication app)
    {
        var toolService = app.Services.CreateScope().ServiceProvider.GetRequiredService<IToolService>();
        var toolWatcher = new ToolWatcher("Tools", toolService);
        toolWatcher.StartWatching();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevTools API V1"));
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<JwtMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    // Method moved from original code to match the previous implementation
    private static async Task CreateDatabaseSchemaAsync(ApplicationDbContext context, ILogger<Program> logger)
    {
        var model = context.Model;
        var sqlBuilder = new StringBuilder();

        // Ensure public schema exists
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

            // Add primary key constraint (single or composite)
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                var pkColumns = string.Join(", ", primaryKey.Properties.Select(p => $"\"{p.GetColumnName(storeObject)}\""));
                sqlBuilder.AppendLine($", PRIMARY KEY ({pkColumns})");
            }

            sqlBuilder.AppendLine(");");
        }

        // Execute the generated SQL
        var sql = sqlBuilder.ToString();
        logger.LogDebug("Generated SQL:\n{0}", sql);

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute SQL: {Sql}", sql);
            throw;
        }
    }

    // 🛠 Fixed Identity Column Handling
    private static string GetPostgresType(IProperty property)
    {
        var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (clrType.IsEnum) return "INTEGER"; // Handle enums as INTEGER

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

        // 🛠 Fix Identity Column Syntax
        if (property.IsPrimaryKey() && clrType == typeof(int) && property.DeclaringEntityType.FindPrimaryKey()?.Properties.Count == 1)
        {
            return "INTEGER GENERATED ALWAYS AS IDENTITY";
        }

        return baseType;
    }

    // Create default admin account
    private static async Task EnsureDefaultAdminExistsAsync(IUserRepository userRepository, ILogger logger)
    {
        const string adminEmail = "admin@devtools.com";
        const string adminUsername = "admin";
        const string adminPassword = "Admin@123"; // ❗ Change this in production

        var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            logger.LogInformation("Creating default admin account...");

            var adminUser = new User
            {
                Username = adminUsername,
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword), // ✅ Secure hashing
                Role = UserRole.Admin,
                IsPremium = true
            };

            await userRepository.AddAsync(adminUser);
            logger.LogInformation("Default admin account created successfully.");
        }
        else
        {
            logger.LogInformation("Default admin already exists.");
        }
    }
}