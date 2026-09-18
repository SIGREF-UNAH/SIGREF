using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Npgsql;
using SIGREF.API;
using SIGREF.API.ServiceDefaults;
using SIGREF.API.Utils;
using SIGREF.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

//=======================================
// CONFIGURACION DEL SISTEMA DE LOGGING
//=======================================
/// <summary>
/// Configura el pipeline de logging de la aplicacion.
/// Se utiliza el formateador SimpleConsole para reducir el ruido visual y 
/// optimizar la legibilidad en entornos de contenedores (Docker/Aspire).
/// Referencia: https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
/// </summary>

// Limpia los proveedores por defecto (Console, Debug, EventSource) para evitar duplicidad de menssajes
builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    // Mantiene cada entrada de log en una sola linea
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";

    // Habilita colores ANSI para distinguir niveles de log (Info, Warning, Error) visualmente
    options.ColorBehavior = LoggerColorBehavior.Enabled;
});

// GESTION DE RUIDO (Log Filtering)

/// <remarks>
/// Se eleva el nivel minimo a 'Warning' en categorías ruidosas del framework 
/// para priorizar los logs de lógica de negocio y errores críticos.
/// </remarks>

// Oculta logs de peticiones HTTP exitosas (200 OK) y middleware interno.
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

// Oculta la ejecución individual de comandos SQL para evitar saturar la consola
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

// Mantiene logs esenciales sobre el arranque y cierre de la aplicacion
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);


// Banner ASCII
ConsoleBanner.Print();

// Licencia QuestPDF
//QuestPDF.Settings.License = LicenseType.Community;

// =============================================================
// RECURSOS EMBEBIDOS (Solo en Desarrollo)
// =============================================================
if (builder.Environment.IsDevelopment())
{
    var names = typeof(Program).Assembly.GetManifestResourceNames();
    // Usamos el logger del builder para mantener consistencia
    var devLogger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Resources");
    devLogger.LogDebug("Recursos embebidos encontrados: {Resources}", string.Join(", ", names));
}

// =============================================================
// SERVICIOS BASE Y ASPIRE
// =============================================================
builder.AddServiceDefaults();

// PostgreSQL + Context Factory
builder.AddNpgsqlDbContext<SIGREFContext>("sigref");
builder.Services.AddDbContextFactory<SIGREFContext>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<NpgsqlDataSource>().OpenConnection());
// MongoDB
builder.AddMongoDBClient("MongoDb");

// HttpClient nombrado para HAPI FHIR con Service Discovery de Aspire
builder.Services.AddHttpClient("hapifhir", client => // ← "hapifhir" exacto
    {
        client.BaseAddress = new Uri("http://hapifhir/fhir/");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddServiceDiscovery()
    .AddStandardResilienceHandler();

// =============================================================
// STARTUP CLASS
// =============================================================
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services, builder);

var app = builder.Build();

// =============================================================
// CONFIGURACIÓN DEL PIPELINE (Middleware)
// =============================================================
app.MapDefaultEndpoints();
startup.Configure(app, app.Environment);

// =============================================================
// MIGRACIONES AUTOMÁTICAS 
// =============================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SIGREFContext>();
    try
    {
        // Verificación rapida de conexion
        if (string.IsNullOrEmpty(builder.Configuration.GetConnectionString("sigref")))
            app.Logger.LogCritical("Error: Cadena de conexion 'sigref' no encontrada.");

        if (context.Database.GetPendingMigrations().Any())
        {
            app.Logger.LogInformation("Migraciones pendientes detectadas. Aplicando...");
            context.Database.Migrate();
            app.Logger.LogInformation("Base de datos actualizada con exito.");
        }
        else
        {
            app.Logger.LogInformation("Base de datos al dia. Sin migraciones pendientes.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error critico durante la migracion de la base de datos.");
        if (!app.Environment.IsDevelopment()) throw;
    }
}

app.Run();