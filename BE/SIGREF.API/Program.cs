using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SIGREF.API;
using SIGREF.API.ServiceDefaults;
using SIGREF.API.Utils;

var builder = WebApplication.CreateBuilder(args);
// ASCII banner
ConsoleBanner.Print();

QuestPDF.Settings.License = LicenseType.Community;
// ===== ENBEBEDED FILES CONFIG =====
if (builder.Environment.IsDevelopment())
{
    var names = typeof(Program).Assembly.GetManifestResourceNames();
    Console.WriteLine(string.Join("\n", names));
}

// ======================

builder.AddServiceDefaults();

// Add PostgreSQL with Aspire integration - DEBE estar en Program.cs, NO en Startup.cs
builder.AddNpgsqlDbContext<SIGREF.API.Database.SIGREFContext>("sigref");
builder.Services.AddDbContextFactory<SIGREF.API.Database.SIGREFContext>();

// Add MongoDB client with Aspire integration
builder.AddMongoDBClient("MongoDb");

// Log configuration sources for debugging
builder.Logging.AddConsole();
var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");

// Debug: Check if connection strings are available
logger.LogInformation("=== CONNECTION STRING DIAGNOSTICS ===");

// Check all connection strings in configuration
var connectionStringsSection = builder.Configuration.GetSection("ConnectionStrings");
foreach (var child in connectionStringsSection.GetChildren())
{
    logger.LogInformation("Found connection string key: {Key}", child.Key);
}

var connectionString = builder.Configuration.GetConnectionString("sigref");
if (string.IsNullOrEmpty(connectionString))
{
    logger.LogWarning("WARNING: Connection string 'sigref' is null or empty. Make sure the app is running through Aspire AppHost.");
    
    // Try to see what's in the configuration
    logger.LogInformation("Checking raw configuration...");
    var allKeys = builder.Configuration.AsEnumerable().Where(x => x.Key.Contains("sigref", StringComparison.OrdinalIgnoreCase));
    foreach (var kvp in allKeys)
    {
        logger.LogInformation("Config Key: {Key} = {Value}", kvp.Key, kvp.Value?.Length > 0 ? $"[{kvp.Value.Length} chars]" : "[empty]");
    }
}
else
{
    logger.LogInformation("Connection string 'sigref' found: {ConnectionStringLength} characters", connectionString.Length);
    // Log the actual connection string for debugging (mask password)
    var maskedConnectionString = connectionString.Contains("Password=") 
        ? System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]*", "Password=***")
        : connectionString;
    logger.LogInformation("Connection string content: {ConnectionString}", maskedConnectionString);
    
    // Also log the raw bytes to see if there are hidden characters
    logger.LogInformation("First 10 characters (hex): {Hex}", 
        string.Join(" ", connectionString.Take(10).Select(c => ((int)c).ToString("X2"))));
}
logger.LogInformation("=== END DIAGNOSTICS ===");

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services, builder);

var app = builder.Build();
app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);
// Apply database migrations automatically
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<SIGREF.API.Database.SIGREFContext>();

    // Apply any pending migrations
    if (context.Database.GetPendingMigrations().Any())
    {
        app.Logger.LogInformation("Applying pending database migrations...");
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    else
    {
        app.Logger.LogInformation("Database is up to date. No pending migrations.");
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while migrating the database.");
    // In production, you might want to throw the exception to prevent startup
    // throw;
}


app.Run();