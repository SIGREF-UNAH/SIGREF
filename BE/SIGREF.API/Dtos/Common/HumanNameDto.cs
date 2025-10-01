using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;

public class HumanNameDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HumanName.NameUse? Use { get; set; } // "official", "maiden", etc.
    public string? Text { get; set; } // Nombre completo para mostrar
    public string? Family { get; set; } // Apellido
    public List<string>? Given { get; set; } // Nombres de pila
    public List<string>? Prefix { get; set; } // "Dr.", "Sr.", etc.
    public List<string>? Suffix { get; set; } // "Jr.", "PhD", etc.
}

