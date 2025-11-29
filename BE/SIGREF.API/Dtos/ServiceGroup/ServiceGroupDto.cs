using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;

namespace SIGREF.API.Dtos.ServiceGroup
{
    public class ServiceGroupDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("code")]
        public CodeableConceptDto? Code { get; set; }

        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("items")]
        public List<HealthcareDto>? Items { get; set; }
    }
}
