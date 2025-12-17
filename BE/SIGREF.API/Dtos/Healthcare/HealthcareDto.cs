#nullable enable
using SIGREF.API.Dtos.Common;
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
        public string? Comment { get; set; } //descripcion

        [JsonPropertyName("specialty")]
        public List<CodeableConceptDto>? Specialty { get; set; }

        [JsonPropertyName("providedBy")]
        public ReferenceDto? ProvidedBy { get; set; }

        [JsonPropertyName("location")]
        public List<ReferenceDto>? Location { get; set; }

        // =========================
        //   PROPIEDADES DE NEGOCIO
        // =========================

        [JsonPropertyName("abbreviation")]
        public string? Abbreviation { get; set; }

        /// <summary>
        /// Alcance del servicio médico.
        /// Internal  => servicio técnico / no visible
        /// External  => visible al usuario
        /// </summary>
        [JsonPropertyName("scope")]
        public HealthcareScope Scope { get; set; }

        /// <summary>
        /// Costo del servicio médico.
        /// Proviene de SIGREF, no de FHIR.
        /// </summary>
        [JsonPropertyName("cost")]
        public decimal? Cost { get; set; }

        // =========================
        //   METADATA
        // =========================

        [JsonPropertyName("lastUpdated")]
        public DateTime? LastUpdated { get; set; }
    }
}