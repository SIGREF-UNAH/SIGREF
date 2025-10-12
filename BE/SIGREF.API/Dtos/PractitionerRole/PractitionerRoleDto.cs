using Hl7.Fhir.Model;
#nullable enable
using SIGREF.API.Dtos.Common;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.PractitionerRole;

public class PractitionerRoleDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("identifier")]
    public List<IdentifierDto>? Identifier { get; set; } = new();

    [JsonPropertyName("active")]
    public bool? Active { get; set; } = true;

    [JsonPropertyName("period")]
    public PeriodDto? Period { get; set; }   // periodo de validez

    [JsonPropertyName("practitioner")]
    public ReferenceDto? Practitioner { get; set; }

    [JsonPropertyName("organization")]
    public ReferenceDto? Organization { get; set; }

    [JsonPropertyName("network")]
    public List<ReferenceDto>? Network { get; set; } = new();

    [JsonPropertyName("code")]
    public List<CodeableConceptDto> Code { get; set; } = new();

    [JsonPropertyName("specialty")]
    public List<CodeableConceptDto>? Specialty { get; set; } = new();

    [JsonPropertyName("location")]
    public List<ReferenceDto>? Location { get; set; } = new();

    [JsonPropertyName("healthcareService")]
    public List<ReferenceDto>? HealthcareService { get; set; } = new();

    [JsonPropertyName("contact")]
    public List<ExtendedContactDetailDto>? Contact { get; set; } = new();

    [JsonPropertyName("characteristic")]
    public List<CodeableConceptDto>? Characteristic { get; set; } = new();

    [JsonPropertyName("communication")]
    public List<CodeableConceptDto>? Communication { get; set; } = new();
    [JsonPropertyName("availability")]
    public AvailabilityDto? Availability { get; set; }

    [JsonPropertyName("endpoint")]
    public List<ReferenceDto>? Endpoint { get; set; } = new();

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}
