using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Audit.Models;
using SIGREF.API.Audit.Services;
using SIGREF.API.Constants;

namespace SIGREF.API.Controllers.Audit;

[Route("api/[controller]")]
[ApiController]
public class AuditController(IAuditService auditService) : ControllerBase
{
    /// <returns>Lista paginada de logs de auditoría</returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = RolesConstants.ti)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string action = null,
        [FromQuery] string userId = null,
        [FromQuery] string userName = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        // Validar parámetros
        if (page < 1)
            return BadRequest(new { message = "El número de página debe ser mayor a 0" });

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 100" });

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new { message = "La fecha inicial no puede ser mayor a la fecha final" });

        List<AuditLog> logs;

        // Si se especifica userId, filtrar por usuario y rango de fechas
        if (!string.IsNullOrWhiteSpace(userId))
        {
            logs = await auditService.GetLogsByUserAsync(userId, from, to);
        }
        // Si se especifica userName, filtrar por nombre de usuario y rango de fechas
        else if (!string.IsNullOrWhiteSpace(userName))
        {
            logs = await auditService.GetLogsByUserNameAsync(userName, from, to);
        }
        // Si se especifica action, filtrar por acción y rango de fechas
        else if (!string.IsNullOrWhiteSpace(action))
        {
            logs = await auditService.GetLogsByActionAsync(action.ToLower(), from, to);
        }
        // Si solo se especifica rango de fechas sin action ni userId, obtener todos con filtro de fechas
        else if (from.HasValue || to.HasValue)
        {
            // Obtener todos los logs y filtrar por fechas
            var allLogs = await auditService.GetAllLogsAsync(1, int.MaxValue);
            logs = [.. allLogs.Where(log =>
            {
                if (from.HasValue && log.Timestamp < from.Value)
                    return false;
                if (to.HasValue && log.Timestamp > to.Value)
                    return false;
                return true;
            })];
        }
        else
        {
            // Sin filtros, obtener todos con paginación
            logs = await auditService.GetAllLogsAsync(page, pageSize);
        }

        // Aplicar paginación si se usaron filtros
        if (!string.IsNullOrWhiteSpace(action) || !string.IsNullOrWhiteSpace(userId) || !string.IsNullOrWhiteSpace(userName) || from.HasValue || to.HasValue)
        {
            var totalItems = logs.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            logs = [.. logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)];

            var dtos = logs.Select(AuditLogDto.FromAuditLog);

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1,
                data = dtos
            });
        }

        // Respuesta sin filtros
        var simpleDtos = logs.Select(AuditLogDto.FromAuditLog);
        return Ok(new
        {
            page,
            pageSize,
            data = simpleDtos
        });
    }

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
            return NotFound(new { message = $"Log de auditoría con ID '{id}' no encontrado" });

        var dto = AuditLogDto.FromAuditLog(log);
        return Ok(dto);
    }
}
