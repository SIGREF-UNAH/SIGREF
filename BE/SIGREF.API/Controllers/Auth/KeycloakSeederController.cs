using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Services.Auth;
using SIGREF.API.Dtos.Auth;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class KeycloakSeederController : ControllerBase
{
    private readonly IKeycloakAdminService _kcAdmin;

    public KeycloakSeederController(IKeycloakAdminService kcAdmin)
    {
        _kcAdmin = kcAdmin;
    }

    // ============================================================
    // DEBUG ROLES
    // ============================================================
    //[HttpGet("debug/roles")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //public IActionResult DebugRoles([FromServices] IUserContextService ctx)
    //{
    //    return Ok(new {
    //        Roles = ctx.GetUserRoles()
    //    });
    //}

    // ============================================================
    // CREATE USER
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpPost("create-user")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<KeycloakUserDto>))]
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

        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // GET USER BY KEYCLOAK ID
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-id/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<KeycloakUserDto?>))]
    public async Task<IActionResult> GetUserById(string id)
    {
        var response = await _kcAdmin.GetUserByIdAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // GET USER BY PRACTITIONER ID
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-practitioner/{practitionerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<KeycloakUserDto?>))]
    public async Task<IActionResult> GetUserByPractitionerId(string practitionerId)
    {
        var response = await _kcAdmin.GetUserByPractitionerIdAsync(practitionerId);
        return StatusCode(response.StatusCode, response);
    }

    // ============================================================
    // Ver Relacion , si un practitiones ya tiene usuario
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("exists/practitioner/{practitionerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<bool>))]
    public async Task<IActionResult> PractitionerHasUser(string practitionerId)
    {
        var response = await _kcAdmin.PractitionerHasUserAsync(practitionerId);
        return StatusCode(response.StatusCode, response);
    }
}
