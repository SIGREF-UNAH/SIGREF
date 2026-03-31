using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SIGREF.Common.Types;

/// <summary>
/// Estados operacionales de una ubicación (<c>Location.status</c> en FHIR R4).
/// </summary>
public enum LocationStatus
{
    [EnumMember(Value = "active")]
    [JsonStringEnumMemberName("active")]
    Active,

    [EnumMember(Value = "suspended")]
    [JsonStringEnumMemberName("suspended")]
    Suspended,

    [EnumMember(Value = "inactive")]
    [JsonStringEnumMemberName("inactive")]
    Inactive
}
