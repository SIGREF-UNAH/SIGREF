using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SIGREF.API.Database
{
    // Serializador personalizado para manejar cualquier tipo de _id
    public class FlexibleIdSerializer : SerializerBase<string>
    {
        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            return bsonType switch
            {
                BsonType.ObjectId => context.Reader.ReadObjectId().ToString(),
                BsonType.String => context.Reader.ReadString(),
                BsonType.Int32 => context.Reader.ReadInt32().ToString(),
                BsonType.Int64 => context.Reader.ReadInt64().ToString(),
                BsonType.Double => context.Reader.ReadDouble().ToString(),
                _ => context.Reader.ReadString()
            };
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            if (ObjectId.TryParse(value, out ObjectId objectId))
            {
                context.Writer.WriteObjectId(objectId);
            }
            else
            {
                context.Writer.WriteString(value);
            }
        }
    }

    [BsonIgnoreExtraElements]
    public class AuditLog
    {
        [BsonId]
        [BsonSerializer(typeof(FlexibleIdSerializer))]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // Propiedad interna para manejar BsonValue
        [BsonIgnore]
        [JsonIgnore]
        public BsonValue BsonId 
        { 
            get => Id; 
            set => Id = value?.ToString() ?? string.Empty; 
        }

        [BsonElement("user_id")]
        [BsonIgnoreIfNull]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("username")]
        [BsonIgnoreIfNull]
        public string Username { get; set; } = string.Empty;

        [BsonElement("action")]
        [BsonIgnoreIfNull]
        public string Action { get; set; } = string.Empty;

        [BsonElement("action_type")]
        [BsonIgnoreIfNull]
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
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("additional_data")]
        [BsonIgnoreIfNull]
        public string? AdditionalData { get; set; }
    }
}