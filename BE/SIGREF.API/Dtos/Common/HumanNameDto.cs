using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;

public class HumanNameDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("use")]
    public HumanName.NameUse? Use { get; set; } // "official", "maiden", etc.

    [JsonPropertyName("text")]
    public string Text { get; set; } // Nombre completo para mostrar

    [JsonPropertyName("family")]
    public string Family { get; set; } // Apellido

    [JsonPropertyName("given")]
    public List<string> Given { get; set; } // Nombres de pila

    [JsonPropertyName("prefix")]
    public List<string> Prefix { get; set; } // "Dr.", "Sr.", etc.

    [JsonPropertyName("suffix")]
    public List<string> Suffix { get; set; } // "Jr.", "PhD", etc.
}

