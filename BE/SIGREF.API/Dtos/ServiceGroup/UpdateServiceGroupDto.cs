using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.ServiceGroup
{
    public class UpdateServiceGroupDto
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }
        
        [JsonPropertyName("code")]
        public CodeableConceptDto? Code { get; set; }

        [JsonPropertyName("healthcareServiceIds")]
        public List<string>? HealthcareServiceIds { get; set; }
    }
}
