using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.AuditLog;
using SIGREF.API.Services.AuditLog;

namespace SIGREF.API.Controllers.AuditLog
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Obtiene los logs de auditoría con filtros opcionales
        /// </summary>
        [HttpGet]
        [Authorize(Roles = RolesConstants.admin + "," + RolesConstants.auditor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
        {
            var logs = await _auditLogService.GetLogsAsync(filter);
            return Ok(logs);
        }

        /// <summary>
        /// Obtiene un log de auditoría específico por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = RolesConstants.admin + "," + RolesConstants.auditor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLogById(int id)
        {
            var log = await _auditLogService.GetLogByIdAsync(id);
            if (log == null)
                return NotFound($"Log con id '{id}' no encontrado.");

            return Ok(log);
        }
    }
}