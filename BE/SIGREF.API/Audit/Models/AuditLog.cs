using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SIGREF.API.Audit.Models;

/// <summary>
/// Modelo de auditoría basado en FHIR AuditEvent
/// </summary>
public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string TraceId { get; set; }

    /// <summary>
    /// Tipo de acción: create, update, delete, read
    /// </summary>
    [BsonElement("action")]
    public string Action { get; set; }

    /// <summary>
    /// Tipo de recurso FHIR (Patient, Practitioner, etc.)
    /// </summary>
    [BsonElement("resourceType")]
    public string ResourceType { get; set; }

    /// <summary>
    /// ID del recurso afectado
    /// </summary>
    [BsonElement("resourceId")]
    public string ResourceId { get; set; }

    /// <summary>
    /// Usuario que realizó la acción
    /// </summary>
    [BsonElement("userId")]
    public string UserId { get; set; }

    /// <summary>
    /// Nombre del usuario
    /// </summary>
    [BsonElement("userName")]
    public string UserName { get; set; }

    /// <summary>
    /// Fecha y hora de la acción
    /// </summary>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Endpoint HTTP llamado
    /// </summary>
    [BsonElement("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    /// Método HTTP (GET, POST, PUT, DELETE)
    /// </summary>
    [BsonElement("httpMethod")]
    public string HttpMethod { get; set; }

    /// <summary>
    /// Código de estado HTTP de la respuesta
    /// </summary>
    [BsonElement("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>
    /// Datos antes del cambio (para update/delete) - Objeto JSON
    /// </summary>
    [BsonElement("dataBefore")]
    [BsonIgnoreIfNull]
    public BsonDocument DataBefore { get; set; }

    /// <summary>
    /// Datos después del cambio (para create/update) - Objeto JSON
    /// </summary>
    [BsonElement("dataAfter")]
    [BsonIgnoreIfNull]
    public BsonDocument DataAfter { get; set; }

    /// <summary>
    /// Información adicional
    /// </summary>
    [BsonElement("additionalInfo")]
    public Dictionary<string, string> AdditionalInfo { get; set; }

    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    [BsonElement("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje de error si la operación falló
    /// </summary>
    [BsonElement("errorMessage")]
    public string ErrorMessage { get; set; }
}
