using Microsoft.AspNetCore.Mvc;

namespace SIGREF.API.Controllers.Seeder;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly RolesAdminSeeder _rolesSeeder;
    private readonly TiposUbicacionSeeder _ubicacionSeeder;
    private readonly PractitionerRolesSeeder _practitionerRolesSeeder;

    public SeedController(
        RolesAdminSeeder rolesSeeder,
        TiposUbicacionSeeder ubicacionSeeder,
        PractitionerRolesSeeder practitionerRoleSeeder)
    {
        _rolesSeeder = rolesSeeder;
        _ubicacionSeeder = ubicacionSeeder;
        _practitionerRolesSeeder = practitionerRoleSeeder;
    }

    [HttpPost("roles")]
    public async Task<IActionResult> SeedRoles(CancellationToken ct = default)
    {
        await _rolesSeeder.SeedAsync(ct);
        return Ok("Roles sembrados.");
    }
    [HttpPost("ubicaciones")]
    public async Task<IActionResult> SeedUbicaciones(CancellationToken ct = default)
    {
        await _ubicacionSeeder.SeedAsync(ct);
        return Ok("Tipos de ubicación sembrados.");
    }

    [HttpPost("all-manual")]
    public async Task<IActionResult> SeedAllManual(CancellationToken ct = default)
    {
        await _rolesSeeder.SeedAsync(ct);
        await _ubicacionSeeder.SeedAsync(ct);
        return Ok("Todos los seeders ejecutados manualmente.");
    }

    [HttpPost("practitioner-roles")]
    public async Task<IActionResult> SeedPractitionerRoles(CancellationToken ct = default)
    {
        await _practitionerRolesSeeder.SeedAsync(ct);
        return Ok("Roles FHIR Practitioner sembrados.");
    }
}

