using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.UserLink;
using SIGREF.API.Services.Auth;

namespace SIGREF.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
// Aclaro que no es un seed de datos en si
// Utiliza un termino llamado seeden ya que un usuario interno crea los usuarios para keyckoal y edita las cosas
public class KeycloakSeederController : ControllerBase
{
    private readonly KeycloakAdminService _kcAdmin;

    public KeycloakSeederController(KeycloakAdminService kcAdmin)
    {
        _kcAdmin = kcAdmin;
    }

    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        var response = await _kcAdmin.CreateUserAsync(
            User,
            dto.Username,
            dto.PractitionerId,
            dto.Email,
            dto.Password,
            dto.Roles
        );

        if (!response.Status)
            return StatusCode(response.StatusCode, new
            {
                message = response.Message
            });

        return Ok(new
        {
            message = response.Message,
            user = response.Data
        });
    }
    
    
    // BUSCAR USUARIOS (SOLO TABLA LOCAL)
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? search = null,
        [FromQuery] bool? enabled = null,
        [FromQuery] int first = 0,
        [FromQuery] int max = 20)
    {
        var response = await _kcAdmin.SearchUsersAsync(
            search,
            enabled,
            first,
            max
        );

        return StatusCode(response.StatusCode, response);
    }
}

