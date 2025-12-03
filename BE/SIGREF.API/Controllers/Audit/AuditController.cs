using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Audit.Services;
using SIGREF.API.Constants;

namespace SIGREF.API.Controllers.Audit;

[Route("api/[controller]")]
[ApiController]
public class AuditController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await auditService.GetAllLogsAsync(page, pageSize);
        var dtos = logs.Select(SIGREF.API.Audit.Models.AuditLogDto.FromAuditLog).ToList();
        return Ok(new
        {
            page,
            pageSize,
            data = dtos
        });
    }

    /// <summary>
    /// Obtener un log de auditoría por su ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        var log = await auditService.GetLogByIdAsync(id);
        if (log == null)
            return NotFound(new { message = "Log no encontrado" });

        var dto = SIGREF.API.Audit.Models.AuditLogDto.FromAuditLog(log);
        return Ok(dto);
    }

    /// <summary>
    /// Obtener logs por acción (create, update, delete, read, login, login-failed)
    /// </summary>
    [HttpGet("action/{action}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByAction(string action, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var logs = await auditService.GetLogsByActionAsync(action, from, to);
        var dtos = logs.Select(SIGREF.API.Audit.Models.AuditLogDto.FromAuditLog).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Obtener logs por código de estado HTTP
    /// </summary>
    [HttpGet("status/{statusCode}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByStatusCode(int statusCode, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var logs = await auditService.GetLogsByStatusCodeAsync(statusCode, from, to);
        var dtos = logs.Select(SIGREF.API.Audit.Models.AuditLogDto.FromAuditLog).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Ver información del token actual (para debug - ELIMINAR EN PRODUCCIÓN)
    /// </summary>
    [HttpGet("debug/token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DebugToken()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var hasTiRole = User.IsInRole(RolesConstants.ti);
        
        return Ok(new
        {
            isAuthenticated,
            hasTiRole,
            tiRoleConstant = RolesConstants.ti,
            roles,
            allClaims = claims
        });
    }

    /// <summary>
    /// Limpiar todos los logs (para testing - ELIMINAR EN PRODUCCIÓN)
    /// </summary>
    [HttpDelete("test/clear")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAllLogs()
    {
        await auditService.ClearAllLogsAsync();
        return Ok(new { message = "Todos los logs han sido eliminados" });
    }
}
