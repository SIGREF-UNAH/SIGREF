using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SIGREF.API.Constants;
using SIGREF.API.Services.AuditLog;
using SIGREF.API.Database;

namespace SIGREF.API.Controllers.AuditLog
{
    /// <summary>
    /// Controlador para gestionar eventos de auditoría
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
   
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los registros de auditoría con filtros (sin paginación)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string eventType = null,
            [FromQuery] string entityName = null,
            [FromQuery] string entityId = null,
            [FromQuery] string userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo logs de auditoría - EventType: {EventType}, EntityName: {EntityName}, EntityId: {EntityId}, UserId: {UserId}",
                    eventType, entityName, entityId, userId);

                var logs = await _auditLogService.GetAllAuditLogsAsync(
                    eventType, entityName, entityId, userId, fromDate, toDate);

                _logger.LogInformation("Se encontraron {Count} registros de auditoría", logs?.Count ?? 0);

                return Ok(new
                {
                    success = true,
                    count = logs?.Count ?? 0,
                    data = logs ?? new List<Database.AuditLog>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs de auditoría");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener logs de auditoría",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Endpoint de diagnóstico para verificar la conexión a MongoDB
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var count = await _auditLogService.GetAuditLogsCountAsync();

                return Ok(new
                {
                    status = "healthy",
                    mongodb = "connected",
                    totalLogs = count,
                    database = "sigref_logs",
                    collection = "audit_logs",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    mongodb = "disconnected",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Crea un log de prueba para verificar que funciona
        /// </summary>
        [HttpPost("test")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTestLog()
        {
            try
            {
                var testLog = await _auditLogService.CreateEventLogAsync(
                    eventType: "Test",
                    entityName: "TestEntity",
                    entityId: "test-123",
                    userId: "test-user",
                    userName: "Test User",
                    statusCode: 200,
                    requestMethod: "POST",
                    requestPath: "/api/auditlog/test",
                    oldValues: null,
                    newValues: new { message = "Test log created at " + DateTime.UtcNow },
                    errorMessage: null,
                    executionTimeMs: 0,
                    additionalData: new Dictionary<string, object>
                    {
                        { "isTest", true },
                        { "createdAt", DateTime.UtcNow }
                    }
                );

                return Created($"/api/auditlog/entity/{testLog.Id}", new
                {
                    success = true,
                    message = "Log de prueba creado exitosamente",
                    logId = testLog.Id,
                    log = testLog
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear log de prueba",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Obtiene registros de auditoría por ID de entidad
        /// </summary>
        [HttpGet("entity/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAuditLogsByEntity(string id)
        {
            try
            {
                var logs = await _auditLogService.GetAuditLogsByEntityAsync(id);

                if (logs == null || logs.Count == 0)
                    return NotFound(new
                    {
                        success = false,
                        message = "No se encontraron registros de auditoría para esta entidad",
                        entityId = id
                    });

                return Ok(new
                {
                    success = true,
                    count = logs.Count,
                    entityId = id,
                    data = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por entidad {EntityId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener logs",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de eventos de auditoría
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var stats = await _auditLogService.GetEventStatisticsAsync(fromDate, toDate);

                return Ok(new
                {
                    success = true,
                    statistics = stats,
                    totalEvents = stats.Values.Sum(),
                    fromDate = fromDate,
                    toDate = toDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener estadísticas",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene el conteo total de registros según filtros
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogsCount(
            [FromQuery] string eventType = null,
            [FromQuery] string entityName = null,
            [FromQuery] string entityId = null,
            [FromQuery] string userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var count = await _auditLogService.GetAuditLogsCountAsync(
                    eventType, entityName, entityId, userId, fromDate, toDate);

                return Ok(new
                {
                    success = true,
                    count = count,
                    filters = new
                    {
                        eventType,
                        entityName,
                        entityId,
                        userId,
                        fromDate,
                        toDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener conteo",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene todos los tipos de eventos disponibles
        /// </summary>
        [HttpGet("event-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetEventTypes()
        {
            return Ok(new
            {
                success = true,
                eventTypes = AuditEventTypes.AllTypes,
                count = AuditEventTypes.AllTypes.Length
            });
        }
    }
}