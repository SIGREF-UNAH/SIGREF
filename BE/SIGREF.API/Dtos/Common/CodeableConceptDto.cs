using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;
public class CodeableConceptDto
{

    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("coding")]
    public List<CodingDto>? Coding { get; set; } = null;


}

