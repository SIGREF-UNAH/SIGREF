#nullable enable
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Healthcare
{
    public class HealthcareDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("identifier")]
        public List<IdentifierDto>? Identifier { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; } = true;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("comment")]
        public string? Comment { get; set; } // descripción

        [JsonPropertyName("specialty")]
        public List<CodeableConceptDto>? Specialty { get; set; }

        [JsonPropertyName("providedBy")]
        public ReferenceDto? ProvidedBy { get; set; } // organización que presta el servicio

        [JsonPropertyName("location")]
        public List<ReferenceDto>? Location { get; set; } // áreas donde se presta el servicio

        [JsonPropertyName("extension")]
        public List<ExtensionDto>? Extension { get; set; } // para abreviación y costo

        [JsonPropertyName("lastUpdated")]
        public DateTime? LastUpdated { get; set; }
    }
}
