using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SIGREF.API.Audit.Dto;

/// <summary>
///     Modelo de auditoría de capa API (Sustituye al antiguo modelo basado en FHIR AuditEvent)
/// </summary>
public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("traceId")] public string TraceId { get; set; }

    [BsonElement("action")] public string Action { get; set; } // "CREATE", "UPDATE", "DELETE", "READ"

    [BsonElement("resourceType")] public string ResourceType { get; set; } // "Patient", "Practitioner", etc.

    [BsonElement("resourceId")] public string ResourceId { get; set; }

    [BsonElement("userId")] public string UserId { get; set; }

    [BsonElement("userName")] public string UserName { get; set; }

    [BsonElement("ipAddress")] public string IpAddress { get; set; } // Vital para auditorías reales

    [BsonElement("timestamp")] public DateTime Timestamp { get; set; }

    [BsonElement("endpoint")] public string Endpoint { get; set; }

    [BsonElement("httpMethod")] public string HttpMethod { get; set; }

    [BsonElement("statusCode")] public int StatusCode { get; set; }

    // --- REEMPLAZO DE DATABEFORE / DATAAFTER ---

    /// <summary>
    ///     Lista de las rutas JSON que fueron modificadas (POST/PUT/PATCH).
    /// </summary>
    [BsonElement("affectedKeys")]
    [BsonIgnoreIfNull]
    public List<string> AffectedKeys { get; set; }

    /// <summary>
    ///     Filtros o query parameters utilizados en consultas (GET).
    /// </summary>
    [BsonElement("filtersUsed")]
    [BsonIgnoreIfNull]
    public Dictionary<string, string> FiltersUsed { get; set; }

    // --- ESTADO Y METADATA ---

    [BsonElement("success")] public bool Success { get; set; }

    [BsonElement("errorMessage")]
    [BsonIgnoreIfNull]
    public string ErrorMessage { get; set; }

    [BsonElement("errorCode")]
    [BsonIgnoreIfNull]
    public string ErrorCode { get; set; }

    [BsonElement("additionalInfo")]
    [BsonIgnoreIfNull]
    public Dictionary<string, string> AdditionalInfo { get; set; }
}
