using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGREF.API.Database
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("username")]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("action")]
        [MaxLength(500)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [Column("action_type")]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // CREATE, READ, UPDATE, DELETE, ERROR

        [Column("endpoint")]
        [MaxLength(500)]
        public string? Endpoint { get; set; }

        [Column("http_method")]
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        [Column("status_code")]
        public int? StatusCode { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("request_body")]
        public string? RequestBody { get; set; }

        [Column("response_body")]
        public string? ResponseBody { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("additional_data")]
        public string? AdditionalData { get; set; }
    }
}