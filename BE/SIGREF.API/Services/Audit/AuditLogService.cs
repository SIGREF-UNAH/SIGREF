// Services/AuditLogService.cs

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SIGREF.API.Audit.Dto;
using SIGREF.API.Dtos.Audit;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly IMongoCollection<AuditLog> _auditLogs;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IMongoClient mongoClient,
        ILogger<AuditLogService> logger)
    {
        var database = mongoClient.GetDatabase("SIGREF_Audit");
        _auditLogs = database.GetCollection<AuditLog>("Logs");
        _logger = logger;
    }

    public async Task<PagedResultDto<AuditLog>> GetAuditLogsAsync(AuditLogFilterDto filter)
    {
        try
        {
            // Agrega esto temporalmente para depurar
            var databaseName = _auditLogs.Database.DatabaseNamespace.DatabaseName;
            _logger.LogWarning("Conectado a la base de datos: {DatabaseName}", databaseName);
            var allCount = await _auditLogs.CountDocumentsAsync(_ => true);
            _logger.LogWarning("Total documentos en colección: {Count}", allCount);
            // Construir el filtro de MongoDB
            var filterBuilder = Builders<AuditLog>.Filter;
            var filters = new List<FilterDefinition<AuditLog>>();

            // Filtros específicos
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.TraceId, filter.TraceId);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.Action, filter.Action.ToString());
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.ResourceType, filter.ResourceType);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.ResourceId, filter.ResourceId);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.UserId, filter.UserId);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.UserName, filter.UserName);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.IpAddress, filter.IpAddress);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.Endpoint, filter.Endpoint);
            AddStringFilterIfNotNull(filters, filterBuilder, f => f.HttpMethod, filter.HttpMethod);

            if (filter.StatusCode.HasValue) filters.Add(filterBuilder.Eq(f => f.StatusCode, filter.StatusCode.Value));

            if (filter.Success.HasValue) filters.Add(filterBuilder.Eq(f => f.Success, filter.Success.Value));

            // Filtro por rango de fechas
            if (filter.FromDate.HasValue) filters.Add(filterBuilder.Gte(f => f.Timestamp, filter.FromDate.Value));

            if (filter.ToDate.HasValue) filters.Add(filterBuilder.Lte(f => f.Timestamp, filter.ToDate.Value));

            // Búsqueda por texto en múltiples campos
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchFilter = BuildSearchFilter(filterBuilder, filter.SearchTerm);
                filters.Add(searchFilter);
            }

            // Combinar todos los filtros
            var combinedFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            // Construir la ordenación
            var sortBuilder = Builders<AuditLog>.Sort;
            var sort = BuildSortDefinition(sortBuilder, filter.SortBy, filter.SortDescending);

            // Obtener el total de documentos que coinciden
            var totalItems = await _auditLogs.CountDocumentsAsync(combinedFilter);

            // Calcular páginas
            var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

            // Validar página actual
            var currentPage = Math.Max(1, Math.Min(filter.CurrentPage, Math.Max(1, totalPages)));

            // Calcular el skip para la paginación
            var skip = (currentPage - 1) * filter.PageSize;

            // Ejecutar la consulta con paginación
            var items = await _auditLogs
                .Find(combinedFilter)
                .Sort(sort)
                .Skip(skip)
                .Limit(filter.PageSize)
                .ToListAsync();

            // Construir la respuesta paginada
            return new PagedResultDto<AuditLog>
            {
                Items = items,
                Pagination = new PaginationDto
                {
                    CurrentPage = currentPage,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = currentPage > 1,
                    HasNext = currentPage < totalPages
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recuperar los logs de auditoría. Filtro: {@Filter}", filter);
            throw;
        }
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(string id)
    {
        try
        {
            return await _auditLogs
                .Find(log => log.Id == id)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recuperar el log de auditoría con ID: {Id}", id);
            throw;
        }
    }

    #region Métodos Privados

    private void AddStringFilterIfNotNull(
        List<FilterDefinition<AuditLog>> filters,
        FilterDefinitionBuilder<AuditLog> filterBuilder,
        Expression<Func<AuditLog, string>> field,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) filters.Add(filterBuilder.Eq(field, value));
    }

    private FilterDefinition<AuditLog> BuildSearchFilter(
        FilterDefinitionBuilder<AuditLog> filterBuilder,
        string searchTerm)
    {
        // Escapar caracteres especiales de regex
        var escapedSearchTerm = Regex.Escape(searchTerm);
        var regex = new BsonRegularExpression(escapedSearchTerm, "i");

        return filterBuilder.Or(
            filterBuilder.Regex(f => f.TraceId, regex),
            filterBuilder.Regex(f => f.Action, regex),
            filterBuilder.Regex(f => f.ResourceType, regex),
            filterBuilder.Regex(f => f.ResourceId, regex),
            filterBuilder.Regex(f => f.UserId, regex),
            filterBuilder.Regex(f => f.UserName, regex),
            filterBuilder.Regex(f => f.IpAddress, regex),
            filterBuilder.Regex(f => f.Endpoint, regex),
            filterBuilder.Regex(f => f.ErrorMessage, regex),
            filterBuilder.Regex(f => f.HttpMethod, regex)
        );
    }

    private SortDefinition<AuditLog> BuildSortDefinition(
        SortDefinitionBuilder<AuditLog> sortBuilder,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLower() switch
        {
            "traceid" => sortDescending
                ? sortBuilder.Descending(f => f.TraceId)
                : sortBuilder.Ascending(f => f.TraceId),
            "action" => sortDescending
                ? sortBuilder.Descending(f => f.Action)
                : sortBuilder.Ascending(f => f.Action),
            "resourcetype" => sortDescending
                ? sortBuilder.Descending(f => f.ResourceType)
                : sortBuilder.Ascending(f => f.ResourceType),
            "resourceid" => sortDescending
                ? sortBuilder.Descending(f => f.ResourceId)
                : sortBuilder.Ascending(f => f.ResourceId),
            "userid" => sortDescending
                ? sortBuilder.Descending(f => f.UserId)
                : sortBuilder.Ascending(f => f.UserId),
            "username" => sortDescending
                ? sortBuilder.Descending(f => f.UserName)
                : sortBuilder.Ascending(f => f.UserName),
            "ipaddress" => sortDescending
                ? sortBuilder.Descending(f => f.IpAddress)
                : sortBuilder.Ascending(f => f.IpAddress),
            "endpoint" => sortDescending
                ? sortBuilder.Descending(f => f.Endpoint)
                : sortBuilder.Ascending(f => f.Endpoint),
            "httpmethod" => sortDescending
                ? sortBuilder.Descending(f => f.HttpMethod)
                : sortBuilder.Ascending(f => f.HttpMethod),
            "statuscode" => sortDescending
                ? sortBuilder.Descending(f => f.StatusCode)
                : sortBuilder.Ascending(f => f.StatusCode),
            "success" => sortDescending
                ? sortBuilder.Descending(f => f.Success)
                : sortBuilder.Ascending(f => f.Success),
            "timestamp" => sortDescending
                ? sortBuilder.Descending(f => f.Timestamp)
                : sortBuilder.Ascending(f => f.Timestamp),
            _ => sortDescending
                ? sortBuilder.Descending(f => f.Timestamp)
                : sortBuilder.Ascending(f => f.Timestamp)
        };
    }

    #endregion
}