using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.ServiceGroup;

/// <summary>
/// DTO simplificado de HealthcareService para uso en ServiceGroup
/// Contiene solo las propiedades esenciales
/// </summary>
public class ServiceGroupHealthcareDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Precio del servicio (vendrá de PostgreSQL)
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
}
