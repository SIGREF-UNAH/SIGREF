using MongoDB.Bson;

namespace SIGREF.API.Audit.Models;

/// <summary>
/// DTO para serializar AuditLog a JSON
/// </summary>
public class AuditLogDto
{
    public string Id { get; set; }
    public string Action { get; set; }
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; }
    public string HttpMethod { get; set; }
    public int StatusCode { get; set; }
    public object DataBefore { get; set; }
    public object DataAfter { get; set; }
    public Dictionary<string, string> AdditionalInfo { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }

    public static AuditLogDto FromAuditLog(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            Action = log.Action,
            ResourceType = log.ResourceType,
            ResourceId = log.ResourceId,
            UserId = log.UserId,
            Timestamp = log.Timestamp,
            Endpoint = log.Endpoint,
            HttpMethod = log.HttpMethod,
            StatusCode = log.StatusCode,
            DataBefore = log.DataBefore != null ? BsonTypeMapper.MapToDotNetValue(log.DataBefore) : null,
            DataAfter = log.DataAfter != null ? BsonTypeMapper.MapToDotNetValue(log.DataAfter) : null,
            AdditionalInfo = log.AdditionalInfo,
            Success = log.Success,
            ErrorMessage = log.ErrorMessage
        };
    }
}
