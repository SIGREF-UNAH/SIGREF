using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SIGREF.API.Database
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("user_id")]
        [BsonRequired]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("username")]
        [BsonRequired]
        public string Username { get; set; } = string.Empty;

        [BsonElement("action")]
        [BsonRequired]
        public string Action { get; set; } = string.Empty;

        [BsonElement("action_type")]
        [BsonRequired]
        public string ActionType { get; set; } = string.Empty;

        [BsonElement("endpoint")]
        [BsonIgnoreIfNull]
        public string? Endpoint { get; set; }

        [BsonElement("http_method")]
        [BsonIgnoreIfNull]
        public string? HttpMethod { get; set; }

        [BsonElement("status_code")]
        [BsonIgnoreIfNull]
        public int? StatusCode { get; set; }

        [BsonElement("error_message")]
        [BsonIgnoreIfNull]
        public string? ErrorMessage { get; set; }

        [BsonElement("request_body")]
        [BsonIgnoreIfNull]
        public string? RequestBody { get; set; }

        [BsonElement("response_body")]
        [BsonIgnoreIfNull]
        public string? ResponseBody { get; set; }

        [BsonElement("ip_address")]
        [BsonIgnoreIfNull]
        public string? IpAddress { get; set; }

        [BsonElement("user_agent")]
        [BsonIgnoreIfNull]
        public string? UserAgent { get; set; }

        [BsonElement("timestamp")]
        [BsonRequired]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("additional_data")]
        [BsonIgnoreIfNull]
        public string? AdditionalData { get; set; }
    }
}