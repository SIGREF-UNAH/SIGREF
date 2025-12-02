using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.ServiceGroup;

/// <summary>
/// DTO simplificado de Location para uso en ServiceGroup
/// Contiene solo las propiedades esenciales
/// </summary>
public class ServiceGroupLocationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
