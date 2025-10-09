#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Patient
{
    public class PatientDTO
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("name")]
        public List<HumanNameDto>? Name { get; set; }

        [JsonPropertyName("gender")]
        public AdministrativeGender? Gender { get; set; } // "male", "female", "other", "unknown"

        [JsonPropertyName("birthDate")]
        public DateTime? BirthDate { get; set; }

        [JsonPropertyName("telecom")]
        public List<ContactPointDto>? Telecom { get; set; }

        [JsonPropertyName("address")]
        public List<AddressDto>? Address { get; set; }

        [JsonPropertyName("identifier")]
        public List<IdentifierDto>? Identifier { get; set; }

        [JsonPropertyName("maritalStatus")]
        public string? MaritalStatus { get; set; }

        [JsonPropertyName("lastUpdated")]
        public DateTime? LastUpdated { get; set; }
    }
}
