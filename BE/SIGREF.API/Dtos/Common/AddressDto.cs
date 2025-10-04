using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;
public class AddressDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("use")]
    public Address.AddressUse? Use { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Address.AddressType? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("line")]
    public List<string>? Line { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("district")]
    public string? District { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("country")]
    //TODO: usar libreria en frontend 
    public string? Country { get; set; }
}

