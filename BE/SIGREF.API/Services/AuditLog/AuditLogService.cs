using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SIGREF.API.Database;

namespace SIGREF.API.Services.AuditLog
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IMongoCollection<Database.AuditLog> _auditLogs;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            IMongoClient mongoClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditLogService> logger)
        {
            // ⚠️ IMPORTANTE: Cambiar a los nombres correctos de tu MongoDB
            var database = mongoClient.GetDatabase("sigref_logs");  // Era "SIGREF_Audit"
            _auditLogs = database.GetCollection<Database.AuditLog>("audit_logs");  // Era "AuditLogs"
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            _logger.LogInformation("AuditLogService inicializado. Base de datos: sigref_logs, Colección: audit_logs");

            // Crear índices para mejorar el rendimiento
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                var indexKeys = Builders<Database.AuditLog>.IndexKeys;

                await _auditLogs.Indexes.CreateOneAsync(
                    new CreateIndexModel<Database.AuditLog>(indexKeys.Descending(x => x.Timestamp))
                );

                await _auditLogs.Indexes.CreateOneAsync(
                    new CreateIndexModel<Database.AuditLog>(indexKeys.Ascending(x => x.UserId))
                );

                await _auditLogs.Indexes.CreateOneAsync(
                    new CreateIndexModel<Database.AuditLog>(indexKeys.Ascending(x => x.ActionType))
                );
            }
            catch
            {
                // Los índices ya existen, ignorar
            }
        }

        public async Task<Database.AuditLog> CreateLogAsync(
            string entityName,
            string entityId,
            string action,
            string userId,
            string userName,
            object oldValues,
            object newValues)
        {
            var auditLog = new Database.AuditLog
            {
                ActionType = action,
                Action = action,
                UserId = userId ?? "Anonymous",
                Username = userName ?? "Anonymous",
                RequestBody = oldValues?.ToString(),
                ResponseBody = newValues?.ToString(),
                IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _auditLogs.InsertOneAsync(auditLog);
            return auditLog;
        }

        public async Task<Database.AuditLog> CreateEventLogAsync(
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
            Dictionary<string, object> additionalData = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var auditLog = new Database.AuditLog
            {
                ActionType = eventType,
                Action = requestMethod ?? "UNKNOWN",
                UserId = userId ?? "Anonymous",
                Username = userName ?? "Anonymous",
                StatusCode = statusCode > 0 ? statusCode : null,
                HttpMethod = requestMethod,
                Endpoint = requestPath,
                RequestBody = oldValues?.ToString(),
                ResponseBody = newValues?.ToString(),
                IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context?.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Asignar mensaje de error o datos adicionales según el contexto
            if (statusCode >= 400 || eventType == "Error")
            {
                auditLog.ErrorMessage = errorMessage ?? additionalData?.ToString();
            }
            else
            {
                auditLog.AdditionalData = errorMessage ?? additionalData?.ToString();
            }

            await _auditLogs.InsertOneAsync(auditLog);
            return auditLog;
        }

        public async Task<List<Database.AuditLog>> GetAllAuditLogsAsync(
            string eventType = null,
            string entityName = null,
            string entityId = null,
            string userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                _logger.LogInformation("Buscando logs - EventType: {EventType}, UserId: {UserId}", eventType, userId);

                var filterBuilder = Builders<Database.AuditLog>.Filter;
                var filters = new List<FilterDefinition<Database.AuditLog>>();

                // Solo filtrar por campos que existen en MongoDB
                if (!string.IsNullOrEmpty(eventType))
                    filters.Add(filterBuilder.Eq(x => x.ActionType, eventType));

                if (!string.IsNullOrEmpty(userId))
                    filters.Add(filterBuilder.Eq(x => x.UserId, userId));

                if (fromDate.HasValue)
                    filters.Add(filterBuilder.Gte(x => x.Timestamp, fromDate.Value));

                if (toDate.HasValue)
                    filters.Add(filterBuilder.Lte(x => x.Timestamp, toDate.Value));

                var filter = filters.Count > 0
                    ? filterBuilder.And(filters)
                    : filterBuilder.Empty;

                var result = await _auditLogs.Find(filter)
                    .SortByDescending(x => x.Timestamp)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} registros en MongoDB", result.Count);

                // Los parámetros entityName y entityId se ignoran ya que no existen en Database.AuditLog
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs de MongoDB");
                throw;
            }
        }

        public async Task<long> GetAuditLogsCountAsync(
            string eventType = null,
            string entityName = null,
            string entityId = null,
            string userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var filterBuilder = Builders<Database.AuditLog>.Filter;
            var filters = new List<FilterDefinition<Database.AuditLog>>();

            if (!string.IsNullOrEmpty(eventType))
                filters.Add(filterBuilder.Eq(x => x.ActionType, eventType));

            if (!string.IsNullOrEmpty(userId))
                filters.Add(filterBuilder.Eq(x => x.UserId, userId));

            if (fromDate.HasValue)
                filters.Add(filterBuilder.Gte(x => x.Timestamp, fromDate.Value));

            if (toDate.HasValue)
                filters.Add(filterBuilder.Lte(x => x.Timestamp, toDate.Value));

            var filter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            return await _auditLogs.CountDocumentsAsync(filter);
        }

        public async Task<Dictionary<string, long>> GetEventStatisticsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var filterBuilder = Builders<Database.AuditLog>.Filter;
            var filters = new List<FilterDefinition<Database.AuditLog>>();

            if (fromDate.HasValue)
                filters.Add(filterBuilder.Gte(x => x.Timestamp, fromDate.Value));

            if (toDate.HasValue)
                filters.Add(filterBuilder.Lte(x => x.Timestamp, toDate.Value));

            var filter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var logs = await _auditLogs.Find(filter).ToListAsync();

            return logs
                .GroupBy(x => x.ActionType ?? "Unknown")
                .ToDictionary(g => g.Key, g => (long)g.Count());
        }

        public async Task<List<Database.AuditLog>> GetAuditLogsByEntityAsync(string entityId)
        {
            return await _auditLogs.Find(Builders<Database.AuditLog>.Filter.Empty)
                .SortByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        // Métodos de compatibilidad con el código existente
        public async Task<Database.AuditLog> CreateEventLogAsync(string eventType, string entityName, string entityId, 
            string userId, string userName, object oldValues = null, object newValues = null, string additionalInfo = null,
            int httpStatusCode = 0, string httpMethod = null, string endpointPath = null)
        {
            return await CreateEventLogAsync(
                eventType: eventType,
                entityName: entityName,
                entityId: entityId,
                userId: userId,
                userName: userName,
                statusCode: httpStatusCode,
                requestMethod: httpMethod,
                requestPath: endpointPath,
                oldValues: oldValues,
                newValues: newValues,
                errorMessage: additionalInfo
            );
        }

        public async Task<Database.AuditLog> LogLoginAsync(string userId, string userName, string additionalInfo = null)
        {
            return await CreateEventLogAsync(
                eventType: "Login",
                entityName: "Authentication",
                entityId: userId,
                userId: userId,
                userName: userName,
                statusCode: 200,
                requestMethod: "POST",
                requestPath: "/api/auth/login",
                errorMessage: additionalInfo ?? "Inicio de sesión exitoso"
            );
        }

        public async Task<Database.AuditLog> LogErrorAsync(string errorMessage, string entityName = null, string entityId = null, 
            string userId = null, string userName = null)
        {
            var context = _httpContextAccessor.HttpContext;
            return await CreateEventLogAsync(
                eventType: "Error",
                entityName: entityName ?? "System",
                entityId: entityId,
                userId: userId ?? "System",
                userName: userName ?? "System",
                statusCode: context?.Response.StatusCode ?? 500,
                requestMethod: context?.Request.Method ?? "UNKNOWN",
                requestPath: context?.Request.Path.Value ?? "/unknown",
                errorMessage: errorMessage
            );
        }

        public async Task<List<Database.AuditLog>> GetAuditLogsAsync(string eventType = null, string entityName = null, string entityId = null, 
            string userId = null, DateTime? fromDate = null, DateTime? toDate = null, int skip = 0, int take = 50)
        {
            var allLogs = await GetAllAuditLogsAsync(eventType, entityName, entityId, userId, fromDate, toDate);
            return allLogs.Skip(skip).Take(take).ToList();
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }
    }
}