#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;

namespace SIGREF.API.Dtos;

public class CreateLocationDto
{
    [Required] [StringLength(255)] public string Name { get; set; } = string.Empty;

    public string[] Alias { get; set; } = [];

    public string? Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Location.LocationStatus? Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public Location.LocationMode Mode { get; set; }

    public AddressDto? Address { get; set; }

    public List<ContactPointDto> Telecom { get; set; } = new();

    public string? Type { get; set; }
    public string? PartOfId { get; set; }
    public string? ManagingOrganizationIds { get; set; }
}

public class UpdateLocationDto
{
    [StringLength(255)] public string? Name { get; set; }

    public string? Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Location.LocationStatus? Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public Location.LocationMode Mode { get; set; }

    public AddressDto? Address { get; set; }

    public List<ContactPointDto>? Telecom { get; set; }

    public string? Type { get; set; }
}