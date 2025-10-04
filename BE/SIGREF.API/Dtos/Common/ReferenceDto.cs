using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;

public class ReferenceDto
{

    [JsonPropertyName("reference")]
    public string Reference { get; set; } 

    [JsonPropertyName("type")]
    public string Type { get; set; }     
    [JsonPropertyName("identifier")]
    public IdentifierTypeDto Identifier { get; set; }
    [JsonPropertyName("display")]
    public string Display { get; set; }   
}

