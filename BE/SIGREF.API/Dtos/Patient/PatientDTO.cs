#nullable enable
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Patient;

public class PatientDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonPropertyName("active")] public bool Active { get; set; }

    [JsonPropertyName("name")] public List<HumanNameDto>? Name { get; set; }

    [JsonPropertyName("gender")]
    public AdministrativeGender? Gender { get; set; } // "male", "female", "other", "unknown"

    [JsonPropertyName("birthDate")] public DateTime? BirthDate { get; set; }

    [JsonPropertyName("telecom")] public List<ContactPointDto>? Telecom { get; set; }

    [JsonPropertyName("address")] public List<AddressDto>? Address { get; set; }

    [JsonPropertyName("identifier")] public List<IdentifierDto>? Identifier { get; set; }

    [JsonPropertyName("maritalStatus")] public CodeableConceptDto? MaritalStatus { get; set; }

    [JsonPropertyName("extension")] public List<ExtensionDto>? Extension { get; set; }

    [JsonPropertyName("lastUpdated")] public DateTime? LastUpdated { get; set; }
}