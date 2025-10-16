using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Dtos.AuditLog;

namespace SIGREF.API.Services.AuditLog
{
    public class AuditLogService : IAuditLogService
    {
        private readonly SIGREFContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(SIGREFContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(string userId, string username, string action, string actionType,
            string? endpoint = null, string? httpMethod = null, int? statusCode = null,
            string? errorMessage = null, string? requestBody = null, string? responseBody = null,
            string? ipAddress = null, string? userAgent = null, string? additionalData = null)
        {
            try
            {
                var auditLog = new Database.AuditLog
                {
                    UserId = userId,
                    Username = username,
                    Action = action,
                    ActionType = actionType,
                    Endpoint = endpoint,
                    HttpMethod = httpMethod,
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    AdditionalData = additionalData
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el log de auditoría");
            }
        }

        public async Task<List<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
        {
            // Normalizar las fechas: aplica el último mes por defecto si no se proporcionan
            filter.NormalizeDates();

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(l => l.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Username))
                query = query.Where(l => l.Username.Contains(filter.Username));

            if (!string.IsNullOrEmpty(filter.ActionType))
                query = query.Where(l => l.ActionType == filter.ActionType);

            // Después de NormalizeDates(), las fechas siempre tendrán valor
            if (filter.StartDate.HasValue)
                query = query.Where(l => l.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(l => l.Timestamp <= filter.EndDate.Value);

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Username = l.Username,
                    Action = l.Action,
                    ActionType = l.ActionType,
                    Endpoint = l.Endpoint,
                    HttpMethod = l.HttpMethod,
                    StatusCode = l.StatusCode,
                    ErrorMessage = l.ErrorMessage,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    Timestamp = l.Timestamp,
                    AdditionalData = l.AdditionalData
                })
                .ToListAsync();

            return logs;
        }

        public async Task<AuditLogDto?> GetLogByIdAsync(int id)
        {
            var log = await _context.AuditLogs
                .Where(l => l.Id == id)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Username = l.Username,
                    Action = l.Action,
                    ActionType = l.ActionType,
                    Endpoint = l.Endpoint,
                    HttpMethod = l.HttpMethod,
                    StatusCode = l.StatusCode,
                    ErrorMessage = l.ErrorMessage,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    Timestamp = l.Timestamp,
                    AdditionalData = l.AdditionalData
                })
                .FirstOrDefaultAsync();

            return log;
        }
    }
}