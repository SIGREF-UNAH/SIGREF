using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Services.Auth;
using SIGREF.API.Dtos.Auth;

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
    [HttpGet("debug/roles")]
    public IActionResult DebugRoles([FromServices] IUserContextService ctx)
    {
        return Ok(new {
            Roles = ctx.GetUserRoles()
        });
    }

    // ============================================================
    // CREATE USER
    // ============================================================
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

        return StatusCode(response.StatusCode, new
        {
            status = response.Status,
            message = response.Message,
            data = response.Data
        });
    }

    // ============================================================
    // GET USER BY KEYCLOAK ID
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-id/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var response = await _kcAdmin.GetUserByIdAsync(id);

        return StatusCode(response.StatusCode, new
        {
            status = response.Status,
            message = response.Message,
            data = response.Data
        });
    }

    // ============================================================
    // GET USER BY PRACTITIONER ID
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("by-practitioner/{practitionerId}")]
    public async Task<IActionResult> GetUserByPractitionerId(string practitionerId)
    {
        var response = await _kcAdmin.GetUserByPractitionerIdAsync(practitionerId);

        return StatusCode(response.StatusCode, new
        {
            status = response.Status,
            message = response.Message,
            data = response.Data
        });
    }

    // ============================================================
    // CHECK IF PRACTITIONER IS LINKED TO A USER
    // ============================================================
    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("exists/practitioner/{practitionerId}")]
    public async Task<IActionResult> PractitionerHasUser(string practitionerId)
    {
        var response = await _kcAdmin.PractitionerHasUserAsync(practitionerId);

        return StatusCode(response.StatusCode, new
        {
            status = response.Status,
            message = response.Message,
            data = response.Data
        });
    }
}
