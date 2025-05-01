using System.Reflection.PortableExecutable;
using DevTools.DataAccess.Repositories;
using DevTools.DataAccess.Repositories.impl;
using DevTools.Domain.Entities;
using DevTools.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DevTools.DataAccess.Persistence;

public class DatabaseContextSeed(
    ILogger<DatabaseContextSeed> logger,
    IUserRepository userRepository,
    IToolRepository toolRepository,
    IToolGroupRepository toolGroupRepository
)
{
    private readonly ILogger<DatabaseContextSeed> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IToolGroupRepository _toolGroupRepository = toolGroupRepository;
    private readonly IToolRepository _toolRepository = toolRepository;

    public async Task SeedDatabaseAsync(DatabaseContext context)
    {
        try
        {
            await SeedAdminUserAsync();
            await SeedToolGroupsAsync();
            await SeedToolsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedAdminUserAsync()
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

    private async Task SeedToolGroupsAsync()
    {
        var groupNames = new[]
        {
            "Crypto", "Converter", "Development", "Network", "Data",
            "Text", "Images & Videos", "Web", "Math", "Measurement"
        };

        foreach (var name in groupNames)
        {
            var exists = await _toolGroupRepository.GetByNameAsync(name);
            if (exists == null)
            {
                var group = new ToolGroup { Name = name };
                await _toolGroupRepository.AddAsync(group);
                _logger.LogInformation("ToolGroup '{GroupName}' created.", name);
            }
            else
            {
                _logger.LogInformation("ToolGroup '{GroupName}' already exists.", name);
            }
        }
    }

    private async Task SeedToolsAsync()
    {
        var tools = new[]
        {
            new {
                Name = "Token generator",
                Description = "Generate random string with the chars you want, uppercase or lowercase letters, numbers and/or symbols.",
                Group = "Crypto",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/TokenGeneratorTool.dll"
            },
            new {
                Name = "ASCII Art Text Generator",
                Description = "Create ASCII art text with many fonts and styles.",
                Group = "Crypto",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/ASCIIArtGeneratorTool.dll"
            },
            new {
                Name = "Benchmark builder",
                Description = "Generate QR codes with custom text and styles.",
                Group = "Measurement",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/BenchmarkBuilderTool.dll"
            },
            new {
                Name = "Chronometer",
                Description = "Monitor the duration of a thing. Basically a chronometer with simple chronometer features.",
                Group = "Measurement",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/Chronometer.dll"
            },
            new {
                Name = "Currency Formatter",
                Description = "Generate hashes with different algorithms (MD5, SHA1, SHA256, etc.).",
                Group = "Converter",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/CurrencyFormatterTool.dll"
            },
            new {
                Name = "ETA calculator",
                Description = "An ETA (Estimated Time of Arrival) calculator to determine the approximate end time of a task.",
                Group = "Math",
                IsPremium = true,
                IsEnabled = true,
                DllPath = "Tools/ETACalculatorTool.dll"
            },
            new {
                Name = "IBAN validator and parser",
                Description = "Validate and parse IBAN numbers. Check if an IBAN is valid and get its details.",
                Group = "Data",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/IBANValidatorTool.dll"
            },
            new
            {
                Name = "IPv4 range expander",
                Description = "Given a start and an end IPv4 address, this tool calculates a valid IPv4 subnet along with its CIDR notation.",
                Group = "Network",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/IPv4RangeExpanderTool.dll"
            },
            new
            {
                Name = "JSON to CSV",
                Description = "Convert JSON to CSV with automatic header detection.",
                Group = "Development",
                IsPremium = true,
                IsEnabled = true,
                DllPath = "Tools/JsonToCsvTool.dll"
            },
            new
            {
                Name = "JSON to XML",
                Description = "Convert JSON to XML",
                Group = "Converter",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/JsonToXmlTool.dll"
            },
            new
            {
                Name = "Lorem ipsum generator",
                Description = "Lorem ipsum is a placeholder text commonly used to demonstrate the visual form of a document or a typeface without relying on meaningful content.",
                Group = "Text",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/LoremIpsumGeneratorTool.dll"
            },
            new
            {
                Name = "MAC address lookup",
                Description = "Find the vendor and manufacturer of a device by its MAC address.",
                Group = "Network",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/MacAddressLookupTool.dll"
            },
            new
            {
                Name = "Math evaluator",
                Description = "A calculator for evaluating mathematical expressions. You can use functions like sqrt, cos, sin, abs, etc.",
                Group = "Math",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/MathEvaluatorTool.dll"
            },
            new
            {
                Name = "Phone parser and formatter",
                Description = "Parse, validate and format phone numbers. Get information about the phone number, like the country code, type, etc.",
                Group = "Data",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/PhoneParserTool.dll"
            },
            new
            {
                Name = "QR Code generator",
                Description = "Generate and download a QR code for a URL (or just plain text), and customize the background and foreground colors.",
                Group = "Images & Videos",
                IsPremium = true,
                IsEnabled = true,
                DllPath = "Tools/QRGeneratorTool.dll"
            },
            new
            {
                Name = "SQL prettify and format",
                Description = "Format and prettify your SQL queries online (it supports various SQL dialects).",
                Group = "Development",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/SQLPrettifierTool.dll"
            },
            new
            {
                Name = "IPv4 subnet calculator",
                Description = "Parse your IPv4 CIDR blocks and get all the info you need about your subnet.",
                Group = "Network",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/SubnetCalculatorTool.dll"
            },
            new
            {
                Name = "Temperature converter",
                Description = "Degrees temperature conversions for Kelvin, Celsius, Fahrenheit, Rankine, Delisle, Newton, Réaumur, and Rømer.",
                Group = "Measurement",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/TemperatureConverterTool.dll"
            },
            new
            {
                Name = "Text diff",
                Description = "Compare two texts and see the differences between them.",
                Group = "Text",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/TextDiffTool.dll"
            },
            new
            {
                Name = "UUIDs generator",
                Description = "A Universally Unique Identifier (UUID) is a 128-bit number used to identify information in computer systems. The number of possible UUIDs is 16^32, which is 2^128 or about 3.4x10^38 (which is a lot!).",
                Group = "Crypto",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/UuidGeneratorTool.dll"
            },
            new
            {
                Name = "Hash text",
                Description = "Hash a text string using the function you need : MD5, SHA1, SHA256, SHA224, SHA512, SHA384, SHA3 or RIPEMD160",
                Group = "Crypto",
                IsPremium = false,
                IsEnabled = true,
                DllPath = "Tools/HashGeneratorTool.dll"
            },
        };

        foreach (var t in tools)
        {
            var exists = await _toolRepository.GetByNameAsync(t.Name.Trim());
            if (exists == null)
            {
                var group = await _toolGroupRepository.GetByNameAsync(t.Group.Trim());
                if (group == null)
                {
                    _logger.LogWarning("ToolGroup '{GroupName}' not found. Skipping tool: {ToolName}", t.Group, t.Name);
                    continue;
                }

                var tool = new Tool
                {
                    Name = t.Name,
                    Description = t.Description,
                    Group = group,
                    IsPremium = t.IsPremium,
                    IsEnabled = t.IsEnabled,
                    DllPath = t.DllPath
                };

                await _toolRepository.AddAsync(tool);
                _logger.LogInformation("Tool '{ToolName}' created.", t.Name);
            }
            else
            {
                _logger.LogInformation("Tool '{ToolName}' already exists.", t.Name);
            }
        }
    }
}