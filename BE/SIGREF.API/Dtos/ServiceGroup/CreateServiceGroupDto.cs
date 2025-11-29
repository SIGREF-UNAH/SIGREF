using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.ServiceGroup
{
    public class CreateServiceGroupDto
    {
        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "current";

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = "working";
        
        [JsonPropertyName("code")]
        public CodeableConceptDto? Code { get; set; }

        [JsonPropertyName("healthcareServiceIds")]
        public List<string> HealthcareServiceIds { get; set; } = new();
    }
}
