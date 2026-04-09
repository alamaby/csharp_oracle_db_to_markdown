using DotNetEnv;
using OracleToMarkdown.Config;
using OracleToMarkdown.Services;

namespace OracleToMarkdown;

/// <summary>
/// Main entry point for Oracle-to-Markdown Data Dictionary Generator
/// </summary>
class Program
{
    private const string ENV_FILE = ".env";
    private const string OUTPUT_FILE = "DATA_DICTIONARY.md";

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Oracle-to-Markdown Data Dictionary Generator          ║");
        Console.WriteLine("║   Version 1.0.0                                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Step 1: Load configuration from .env
            Console.WriteLine("[1/4] Loading configuration from .env file...");
            if (!LoadConfiguration(out var config))
            {
                return 1;
            }
            Console.WriteLine("✓ Configuration loaded successfully");
            Console.WriteLine();

            // Step 2: Initialize services
            Console.WriteLine("[2/4] Initializing services...");
            var oracleService = new OracleService(config.GetConnectionString());
            var markdownService = new MarkdownGeneratorService();
            Console.WriteLine("✓ Services initialized");
            Console.WriteLine();

            // Step 3: Validate database connection
            Console.WriteLine("[3/4] Validating database connection...");
            Console.WriteLine($"   Connecting to: {config.Host}:{config.Port}/{config.ServiceName}");
            
            if (!await oracleService.ValidateConnectionAsync())
            {
                Console.Error.WriteLine("✗ Failed to connect to Oracle database");
                Console.Error.WriteLine("   Please check your .env configuration and network connectivity");
                return 1;
            }
            Console.WriteLine();

            // Get schema name for output file
            var schemaName = config.SchemaName;
            if (string.IsNullOrEmpty(schemaName))
            {
                schemaName = await oracleService.GetCurrentSchemaAsync();
            }
            if (string.IsNullOrEmpty(schemaName))
            {
                schemaName = config.UserId; // Fallback to user ID
            }
            Console.WriteLine($"   Current schema: {schemaName}");
            Console.WriteLine();

            // Step 4: Fetch schema and generate documentation
            Console.WriteLine("[4/4] Fetching schema and generating documentation...");
            var tables = await oracleService.FetchAllTableSchemasAsync();
            Console.WriteLine();

            if (tables.Count == 0)
            {
                Console.WriteLine("⚠ No tables found in the database");
                return 0;
            }

            // Generate output filename with schema and timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            var outputFileName = $"DATA_DICTIONARY_{schemaName}_{timestamp}.md";
            
            var markdown = markdownService.GenerateMarkdown(tables, outputFileName);
            await markdownService.WriteToFileAsync(markdown, outputFileName);

            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ✓ SUCCESS: Data dictionary generated successfully!    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.Error.WriteLine("║   ✗ ERROR: Failed to generate data dictionary           ║");
            Console.Error.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.Error.WriteLine();
            Console.Error.WriteLine($"Error details: {ex.Message}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Troubleshooting tips:");
            Console.Error.WriteLine("  1. Check your .env file configuration");
            Console.Error.WriteLine("  2. Verify Oracle database is running and accessible");
            Console.Error.WriteLine("  3. Ensure Oracle.ManagedDataAccess.Core is properly installed");
            Console.Error.WriteLine("  4. Check network/firewall settings for Oracle port");
            
            return 1;
        }
    }

    /// <summary>
    /// Loads and validates configuration from .env file
    /// </summary>
    private static bool LoadConfiguration(out DatabaseConfig config)
    {
        config = new DatabaseConfig();

        // Check if .env file exists
        if (!File.Exists(ENV_FILE))
        {
            Console.Error.WriteLine($"✗ Configuration file '{ENV_FILE}' not found");
            Console.Error.WriteLine($"  Please create a {ENV_FILE} file with your Oracle database credentials");
            Console.Error.WriteLine("  See README.md for configuration example");
            return false;
        }

        // Load .env file
        Env.Load(ENV_FILE);

        // Map environment variables to configuration
        config.Host = Env.GetString("ORACLE_HOST", "");
        config.Port = Env.GetInt("ORACLE_PORT", 1521);
        config.ServiceName = Env.GetString("ORACLE_SERVICE_NAME", "");
        config.UserId = Env.GetString("ORACLE_USER_ID", "");
        config.Password = Env.GetString("ORACLE_PASSWORD", "");
        config.SchemaName = Env.GetString("ORACLE_SCHEMA", "");

        // Validate configuration
        if (!config.Validate(out var errors))
        {
            Console.Error.WriteLine("✗ Invalid configuration:");
            foreach (var error in errors)
            {
                Console.Error.WriteLine($"  - {error}");
            }
            return false;
        }

        return true;
    }
}
