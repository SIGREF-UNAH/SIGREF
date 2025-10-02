using System.Text.Json.Serialization;
using Hl7.Fhir.Model;

namespace SIGREF.API.Dtos.Common;

public class IdentifierDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("use")]
    public Identifier.IdentifierUse? Use { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; } // Texto: "DNI", "Pasaporte", etc.

    [JsonPropertyName("system")]
    public string System { get; set; } // URI del sistema emisor

    [JsonPropertyName("value")]
    public string Value { get; set; } // Valor del ID
}

