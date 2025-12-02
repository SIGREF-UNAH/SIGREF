using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.ServiceGroup;

public class UpdateServiceGroupDto
{
    [JsonPropertyName("title")] public string? Title { get; set; }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List.ListStatus Status { get; set; } = List.ListStatus.Current;


    [JsonPropertyName("code")] public CodeableConceptDto? Code { get; set; }

    [JsonPropertyName("healthcareServiceIds")]
    public List<string>? HealthcareServiceIds { get; set; }

    [JsonPropertyName("locationIds")] public List<string>? LocationIds { get; set; }
    
    [JsonPropertyName("description")] public string? Description { get; set; }
}