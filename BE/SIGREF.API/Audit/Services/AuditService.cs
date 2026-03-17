using MongoDB.Driver;
using SIGREF.API.Audit.Models;
using SIGREF.API.Dtos.Audit;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Audit.Services;

public class AuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _auditCollection;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger, IMongoClient client)
    {
        var database = client.GetDatabase("MongoDb");
        _auditCollection = database.GetCollection<AuditLog>("audit_logs");
        _logger = logger;

        // Crear índices para mejorar el rendimiento de las consultas
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            var indexKeys = Builders<AuditLog>.IndexKeys
                .Ascending(x => x.ResourceType)
                .Ascending(x => x.ResourceId);
            _auditCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(indexKeys));

            var userIndexKeys = Builders<AuditLog>.IndexKeys.Ascending(x => x.UserId);
            _auditCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(userIndexKeys));

            var timestampIndexKeys = Builders<AuditLog>.IndexKeys.Descending(x => x.Timestamp);
            _auditCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(timestampIndexKeys));

            var actionIndexKeys = Builders<AuditLog>.IndexKeys.Ascending(x => x.Action);
            _auditCollection.Indexes.CreateOne(new CreateIndexModel<AuditLog>(actionIndexKeys));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron crear índices de auditoría");
        }
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        try
        {
            auditLog.Timestamp = DateTime.UtcNow;
            await _auditCollection.InsertOneAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar log de auditoría");
        }
    }

    public async Task<AuditLog> GetLogByIdAsync(string id)
    {
        var filter = Builders<AuditLog>.Filter.Eq(x => x.Id, id);
        return await _auditCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task LogLoginAsync(string userId, string userName, List<string> roles, string clientIp, bool success, string errorMessage = null)
    {
        var auditLog = new AuditLog
        {
            Action = success ? "login" : "login-failed",
            ResourceType = "Authentication",
            ResourceId = userId,
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Endpoint = "/auth/login",
            HttpMethod = "POST",
            StatusCode = success ? 200 : 401,
            Success = success,
            ErrorMessage = errorMessage,
            AdditionalInfo = new Dictionary<string, string>
            {
                { "eventType", "authentication" }
            }
        };

        await LogAsync(auditLog);
    }
    public async Task<ResponseDto<PagedResultDto<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query)
    {
        try
        {
            var filterBuilder = Builders<AuditLog>.Filter;
            var filter = filterBuilder.Empty;

            // Aplicar filtros según los parámetros de búsqueda
            if (!string.IsNullOrEmpty(query.Action))
                filter &= filterBuilder.Eq(x => x.Action, query.Action);

            if (!string.IsNullOrEmpty(query.UserId))
                filter &= filterBuilder.Eq(x => x.UserId, query.UserId);

            if (!string.IsNullOrEmpty(query.UserName))
                filter &= filterBuilder.Eq(x => x.UserName, query.UserName);

            if (query.From.HasValue)
                filter &= filterBuilder.Gte(x => x.Timestamp, query.From.Value);

            if (query.To.HasValue)
                filter &= filterBuilder.Lte(x => x.Timestamp, query.To.Value);

            // Contar total de documentos que coinciden con el filtro
            var totalCount = await _auditCollection.CountDocumentsAsync(filter);

            // Calcular paginación
            var skip = (query.Page - 1) * query.PageSize;
            var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            // Obtener los documentos paginados
            var logs = await _auditCollection
                .Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Skip(skip)
                .Limit(query.PageSize)
                .ToListAsync();

            // Convertir a DTOs
            var auditLogDtos = logs.Select(log => AuditLogDto.FromAuditLog(log)).ToList();

            // Crear objeto de respuesta paginada
            var pagedResult = new PagedResultDto<AuditLogDto>
            {
                Items = auditLogDtos,
                Pagination = new PaginationDto
                {
                    CurrentPage = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = query.Page > 1,
                    HasNext = query.Page < totalPages
                }
            };

            return new ResponseDto<PagedResultDto<AuditLogDto>>
            {
                Data = pagedResult,
                Message = "Registros de auditoría obtenidos exitosamente",
                Status = true,
                StatusCode = 200
            };
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Error al obtener logs de auditoría");
            return new ResponseDto<PagedResultDto<AuditLogDto>>
            {
                Status = false,
                Message = $"Error al obtener logs de auditoría: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task ClearAllLogsAsync()
    {
        await _auditCollection.DeleteManyAsync(Builders<AuditLog>.Filter.Empty);
    }
}
