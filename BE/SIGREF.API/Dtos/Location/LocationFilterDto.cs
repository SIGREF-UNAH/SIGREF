#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Location;

public class LocationFilterDto : PagedFilterBase
{
    public string? Name { get; set; }

    /// <summary>
    ///     Estado de la ubicación (active, suspended, inactive).
    /// </summary>
    public LocationFilterStatus? Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LocationFilterStatus
{
    [EnumMember(Value = "active")] Active,

    [EnumMember(Value = "suspended")] Suspended,

    [EnumMember(Value = "inactive")] Inactive
}