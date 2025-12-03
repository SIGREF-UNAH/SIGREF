using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGREF.API.Controllers.Seeder;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeedController : ControllerBase
{
    private readonly RolesAdminSeeder _rolesSeeder;
    private readonly TiposUbicacionSeeder _ubicacionSeeder;

    public SeedController(
        RolesAdminSeeder rolesSeeder,
        TiposUbicacionSeeder ubicacionSeeder)
    {
        _rolesSeeder = rolesSeeder;
        _ubicacionSeeder = ubicacionSeeder;
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
}

