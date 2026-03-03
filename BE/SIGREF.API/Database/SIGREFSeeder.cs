
using System.Threading;

namespace  SIGREF.API.Database;
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

    // Ahora inyectamos los seeders ya creados por DI
    public SIGREFSeeder(
        RolesAdminSeeder rolesAdminSeeder,
        TiposUbicacionSeeder tiposUbicacionSeeder,
        ILogger<SIGREFSeeder> logger)
    {
        _rolesAdminSeeder = rolesAdminSeeder;
        _tiposUbicacionSeeder = tiposUbicacionSeeder;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // _logger.LogInformation("Esperando a que el servidor FHIR esté disponible (90s)...");
        //await Task.Delay(TimeSpan.FromSeconds(90));
        _logger.LogInformation("Iniciando siembra...");
        await _rolesAdminSeeder.SeedAsync(ct);
        await _tiposUbicacionSeeder.SeedAsync(ct);
        _logger.LogInformation("Siembra completada.");
    }
}