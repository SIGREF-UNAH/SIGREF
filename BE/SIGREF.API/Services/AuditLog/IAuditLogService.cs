using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SIGREF.API.Database;

namespace SIGREF.API.Services.AuditLog
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Crea un registro de auditoría (método legacy)
        /// </summary>
        Task<Database.AuditLog> CreateLogAsync(string entityName, string entityId, string action,
            string userId, string userName, object oldValues, object newValues);

        /// <summary>
        /// Crea un registro de auditoría con evento completo
        /// </summary>
        Task<Database.AuditLog> CreateEventLogAsync(
            string eventType,
            string entityName,
            string entityId,
            string userId,
            string userName,
            int statusCode,
            string requestMethod,
            string requestPath,
            object oldValues = null,
            object newValues = null,
            string errorMessage = null,
            long executionTimeMs = 0,
            Dictionary<string, object> additionalData = null);

        /// <summary>
        /// Obtiene todos los registros de auditoría con filtros (sin paginación)
        /// </summary>
        Task<List<Database.AuditLog>> GetAllAuditLogsAsync(
            string eventType = null,
            string entityName = null,
            string entityId = null,
            string userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Obtiene el conteo de registros de auditoría con filtros
        /// </summary>
        Task<long> GetAuditLogsCountAsync(
            string eventType = null,
            string entityName = null,
            string entityId = null,
            string userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Obtiene estadísticas de eventos por tipo
        /// </summary>
        Task<Dictionary<string, long>> GetEventStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Obtiene registros de auditoría por ID de entidad
        /// </summary>
        Task<List<Database.AuditLog>> GetAuditLogsByEntityAsync(string entityId);

        // Métodos de compatibilidad con el código existente
        Task<Database.AuditLog> CreateEventLogAsync(string eventType, string entityName, string entityId, 
            string userId, string userName, object oldValues = null, object newValues = null, string additionalInfo = null,
            int httpStatusCode = 0, string httpMethod = null, string endpointPath = null);

        Task<Database.AuditLog> LogLoginAsync(string userId, string userName, string additionalInfo = null);

        Task<Database.AuditLog> LogErrorAsync(string errorMessage, string entityName = null, string entityId = null, 
            string userId = null, string userName = null);

        Task<List<Database.AuditLog>> GetAuditLogsAsync(string eventType = null, string entityName = null, string entityId = null, 
            string userId = null, DateTime? fromDate = null, DateTime? toDate = null, int skip = 0, int take = 50);
    }
}