using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common
{
    public class CodingDto
    {
        [JsonPropertyName("system")]
        public string? System { get; set; } 

        [JsonPropertyName("code")]
        public string? Code { get; set; }   
        [JsonPropertyName("display")]
        public string? Display { get; set; } 

        [JsonPropertyName("version")]
        public string? Version { get; set; }
        [JsonPropertyName("userSelected")]
        public bool? UserSelected { get; set; }
    }
}
