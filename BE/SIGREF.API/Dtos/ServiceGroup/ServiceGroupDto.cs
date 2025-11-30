using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.ServiceGroup;

public class ServiceGroupDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List.ListStatus? Status { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("code")] public CodeableConceptDto? Code { get; set; }

    [JsonPropertyName("date")] public DateTime? Date { get; set; }

    [JsonPropertyName("items")] public List<ServiceGroupHealthcareDto>? HealthcareService { get; set; }

    [JsonPropertyName("locations")] public List<ServiceGroupLocationDto>? Locations { get; set; }

    /// <summary>
    /// Precio total del ServiceGroup (suma de precios de services activos)
    /// </summary>
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }
}