using MongoDB.Driver;
using SIGREF.API.Audit.Models;

namespace SIGREF.API.Audit.Services;

public class AuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _auditCollection;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IMongoDatabase mongoDatabase, ILogger<AuditService> logger)
    {
        _auditCollection = mongoDatabase.GetCollection<AuditLog>("audit_logs");
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

    public async Task<List<AuditLog>> GetAllLogsAsync(int page = 1, int pageSize = 50)
    {
        // Validar parámetros
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 100) pageSize = 100; // Límite máximo para evitar sobrecarga

        var skip = (page - 1) * pageSize;

        return await _auditCollection
            .Find(Builders<AuditLog>.Filter.Empty)
            .SortByDescending(x => x.Timestamp)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetLogsByResourceAsync(string resourceType, string resourceId)
    {
        var filter = Builders<AuditLog>.Filter.And(
            Builders<AuditLog>.Filter.Eq(x => x.ResourceType, resourceType),
            Builders<AuditLog>.Filter.Eq(x => x.ResourceId, resourceId)
        );

        return await _auditCollection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetLogsByUserAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.UserId, userId);

        if (from.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

        if (to.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

        return await _auditCollection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetLogsByActionAsync(string action, DateTime? from = null, DateTime? to = null)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.Action, action);

        if (from.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

        if (to.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

        return await _auditCollection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetLogsByStatusCodeAsync(int statusCode, DateTime? from = null, DateTime? to = null)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(x => x.StatusCode, statusCode);

        if (from.HasValue)
            filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

        if (to.HasValue)
            filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

        return await _auditCollection
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task LogLoginAsync(string userId, string userName, List<string> roles, string clientIp, bool success, string errorMessage = null)
    {
        var auditLog = new AuditLog
        {
            Action = success ? "login" : "login-failed",
            ResourceType = "Authentication",
            ResourceId = userId,
            UserId = userId,
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

    public async Task ClearAllLogsAsync()
    {
        await _auditCollection.DeleteManyAsync(Builders<AuditLog>.Filter.Empty);
    }
}
