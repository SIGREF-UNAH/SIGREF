using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Audit.Services;
using SIGREF.API.Constants;

namespace SIGREF.API.Controllers.Audit;

[Route("api/[controller]")]
[ApiController]
public class AuditController(IAuditService auditService) : ControllerBase
{
    /// <summary>
    /// Obtener todos los logs de auditoría con paginación
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await auditService.GetAllLogsAsync(page, pageSize);
        return Ok(new
        {
            page,
            pageSize,
            data = logs
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

        return Ok(log);
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
        return Ok(logs);
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
        return Ok(logs);
    }

    /// <summary>
    /// Registrar inicio de sesión (llamar desde el frontend después de autenticarse)
    /// </summary>
   
    
}
