#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Types;
using FhirLocation = Hl7.Fhir.Model.Location;

namespace SIGREF.API.Dtos.Location;

public class CreateLocationDto
{
    [Required][StringLength(255)] public string Name { get; set; } = string.Empty;

    public string[] Alias { get; set; } = [];

    public string? Description { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LocationStatus? Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public FhirLocation.LocationMode Mode { get; set; }

    public AddressDto? Address { get; set; }

    public List<ContactPointDto> Telecom { get; set; } = new();

    public string? Type { get; set; }
    public ReferenceDto? PartOf { get; set; }
    public ReferenceDto? ManagingOrganization { get; set; }
}

public class UpdateLocationDto
{
    [StringLength(255)] public string? Name { get; set; }
    public List<string>? Alias { get; set; }

    public string? Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LocationStatus? Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]

    public FhirLocation.LocationMode Mode { get; set; }

    public AddressDto? Address { get; set; }

    public List<ContactPointDto>? Telecom { get; set; }

    public string? Type { get; set; }
    public ReferenceDto? PartOf { get; set; }
    public ReferenceDto? ManagingOrganization { get; set; }
}