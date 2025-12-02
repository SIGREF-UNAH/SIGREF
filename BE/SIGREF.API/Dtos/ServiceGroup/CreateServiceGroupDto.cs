using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.ServiceGroup;

public class CreateServiceGroupDto
{
    [Required] [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List.ListStatus Status { get; set; } = List.ListStatus.Current;


    [JsonPropertyName("code")] public CodeableConceptDto? Code { get; set; }

    [JsonPropertyName("healthcareServiceIds")]
    public List<string> HealthcareServiceIds { get; set; } = new();

    [JsonPropertyName("locationIds")] public List<string> LocationIds { get; set; } = new();
    public required string Description { get; set; } = string.Empty;
}