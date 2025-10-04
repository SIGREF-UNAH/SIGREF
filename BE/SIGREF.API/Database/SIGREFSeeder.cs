using Microsoft.Extensions.Logging;
using SIGREF.API.Database.Seeding;

using SIGREF.API.Services;

/// <summary>
/// Clase orquestadora de la siembra (seeding) de vocabularios FHIR en el sistema.
/// </summary>
/// <remarks>
/// Esta clase coordina la inicialización de catálogos básicos (CodeSystem y ValueSet)
/// necesarios para la operación del Hospital de Occidente.
/// </remarks>
public class SIGREFSeeder
{
    private readonly RolesAdminSeeder _rolesAdminSeeder;
    private readonly TiposUbicacionSeeder _tiposUbicacionSeeder;
    private readonly ILogger<SIGREFSeeder> _logger;

    /// <summary>
    /// Constructor principal del seeder.
    /// </summary>
    /// <param name="fhirService">Servicio para acceder al cliente FHIR.</param>
    /// <param name="logger">Logger para registrar la actividad del seeding.</param>
    public SIGREFSeeder(
        FhirService fhirService,
        ILoggerFactory loggerFactory
    )
    {
        _rolesAdminSeeder = new RolesAdminSeeder(fhirService, loggerFactory.CreateLogger<RolesAdminSeeder>());
        _tiposUbicacionSeeder = new TiposUbicacionSeeder(fhirService, loggerFactory.CreateLogger<TiposUbicacionSeeder>());
        _logger = loggerFactory.CreateLogger<SIGREFSeeder>();
    }

    /// <summary>
    /// Ejecuta el proceso de siembra de todos los vocabularios necesarios.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación si es necesario.</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Esperando a que el servidor FHIR este disponible (90s)...");
        await Task.Delay(TimeSpan.FromSeconds(90), cancellationToken);

        _logger.LogInformation("Iniciando proceso de siembra de vocabularios...");
        await _rolesAdminSeeder.SeedAsync(cancellationToken);
        await _tiposUbicacionSeeder.SeedAsync(cancellationToken);
        _logger.LogInformation("Proceso de siembra completado.");
    }
}