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
    public bool Active { get; set; } = true;

    [JsonPropertyName("code")]
    public List<CodeableConceptDto> Code { get; set; } = new();

    [JsonPropertyName("practitioner")]
    public ReferenceDto? Practitioner { get; set; }

    [JsonPropertyName("organization")]
    public ReferenceDto? Organization { get; set; }

    [JsonPropertyName("location")]
    public List<ReferenceDto>? Location { get; set; } = new();
    //public List<>? HealthcareService { get; set; } = new();
    //public Period? Period { get; set; }

    //[JsonPropertyName("telecom")]
    //public List<ContactPointDto>? Telecom { get; set; } = new();

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }

    [JsonPropertyName("endpoint")]
    public List<ReferenceDto> Endpoint { get; set; } = new();

    [JsonPropertyName("display")]
    public string? Display { get; set; }

}

