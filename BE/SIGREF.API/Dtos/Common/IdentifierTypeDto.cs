using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;
public class IdentifierTypeDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("use")]
    public Identifier.IdentifierUse? Use { get; set; }

    [JsonPropertyName("type")]
    public CodeableConceptDto? Type { get; set; }

    [JsonPropertyName("system")]

    public string? System { get; set; } 

    [JsonPropertyName("value")]
    public string Value { get; set; } 


}

