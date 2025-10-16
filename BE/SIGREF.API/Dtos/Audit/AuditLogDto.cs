namespace SIGREF.API.Dtos.AuditLog
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string? AdditionalData { get; set; }
    }
}