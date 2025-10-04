using Hl7.Fhir.Model;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Common;

public class ContactPointDto
{
    [JsonPropertyName("system")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ContactPoint.ContactPointSystem? System { get; set; }  // phone, email, fax, etc.

    [JsonPropertyName("value")]
    public string? Value { get; set; } = null;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("use")]
    public ContactPoint.ContactPointUse? Use { get; set; }   // home, work, mobile, etc.
    
    [JsonPropertyName("rank")]
    public int? Rank { get; set; }
}

