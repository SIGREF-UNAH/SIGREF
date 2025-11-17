#nullable enable
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Common.ValidationAtributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;

namespace SIGREF.API.Dtos.Patient;
public class UpdatePatientDto
{
    public bool? Active { get; set; }

    [AtLeastOneNameRequired]
    public List<HumanNameDto>? Name { get; set; }

    [Required(ErrorMessage = "El género es obligatorio.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AdministrativeGender? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public CodeableConceptDto? MaritalStatus { get; set; }

    public List<ExtensionDto>? Extension { get; set; }

    public List<ContactPointDto>? Telecom { get; set; }

    public List<AddressDto>? Address { get; set; }

    public List<IdentifierDto>? Identifier { get; set; }
}
