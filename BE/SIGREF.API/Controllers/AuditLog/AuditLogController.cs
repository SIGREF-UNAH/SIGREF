using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.AuditLog;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Controllers.AuditLog
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
        {
            var logs = await _auditLogService.GetLogsAsync(filter);
            return Ok(logs);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogById(string id)
        {
            if (!int.TryParse(id, out int logId))
                return BadRequest("El ID debe ser un número entero válido.");

            var log = await _auditLogService.GetLogByIdAsync(logId);
            if (log == null)
                return NotFound($"Log con id '{id}' no encontrado.");

            return Ok(log);
        }

        [HttpGet("stats")]
        [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetStatsSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var filter = new AuditLogFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                PageSize = 10000 // Para obtener todos los registros del rango
            };

            var logs = await _auditLogService.GetLogsAsync(filter);

            var summary = new
            {
                TotalAcciones = logs.Count,
                PorTipo = logs.GroupBy(l => l.ActionType)
                    .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList(),
                PorUsuario = logs.GroupBy(l => l.Username)
                    .Select(g => new { Usuario = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(10)
                    .ToList(),
                IniciosSesion = logs.Count(l => l.ActionType == "LOGIN"),
                Errores = logs.Count(l => l.ActionType == "ERROR"),
                RangoFechas = new
                {
                    Inicio = filter.StartDate,
                    Fin = filter.EndDate
                }
            };

            return Ok(summary);
        }
    }
}