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

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces<AuditLogDto>()]
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
        int totalItems;

        // Si se especifica userId, filtrar por usuario y rango de fechas
        if (!string.IsNullOrWhiteSpace(userId))
        {
            (logs, totalItems) = await auditService.GetLogsByUserAsync(userId, from, to);
        }
        // Si se especifica userName, filtrar por nombre de usuario y rango de fechas
        else if (!string.IsNullOrWhiteSpace(userName))
        {
            (logs, totalItems) = await auditService.GetLogsByUserNameAsync(userName, from, to);
        }
        // Si se especifica action, filtrar por acción y rango de fechas
        else if (!string.IsNullOrWhiteSpace(action))
        {
            (logs, totalItems) = await auditService.GetLogsByActionAsync(action.ToLower(), from, to);
        }
        // Sin filtros, obtener todos con paginación directa en MongoDB
        else
        {
            (logs, totalItems) = await auditService.GetAllLogsAsync(page, pageSize);
        }

        // Calcular total de páginas
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Aplicar paginación en memoria solo si se usaron filtros
        if (!string.IsNullOrWhiteSpace(action) || !string.IsNullOrWhiteSpace(userId) || !string.IsNullOrWhiteSpace(userName))
        {
            logs = logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        var dtos = logs.Select(AuditLogDto.FromAuditLog);

        return Ok(new
        {
            currentPage = page,
            pageSize,
            totalItems,
            totalPages,
            hasPrevious = page > 1,
            hasNext = page < totalPages,
            data = dtos
        });
    }


    [HttpDelete("test/clear")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAllLogs()
    {
        await auditService.ClearAllLogsAsync();
        return Ok(new { message = "Todos los logs han sido eliminados" });
    }
}
