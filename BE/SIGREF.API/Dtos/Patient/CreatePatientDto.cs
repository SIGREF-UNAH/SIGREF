#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Common.ValidationAtributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Patient;

public class CreatePatientDto
{
    [Required(ErrorMessage = "Al menos un nombre es obligatorio.")]
    [AtLeastOneNameRequired]
    public List<HumanNameDto> Name { get; set; } = new();

    [Required(ErrorMessage = "El género es obligatorio.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AdministrativeGender? Gender { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime? BirthDate { get; set; }

    public bool Active { get; set; } = true;
    public List<ContactPointDto>? Telecom { get; set; }
    public List<AddressDto>? Address { get; set; }
    public List<IdentifierDto>? Identifier { get; set; }
}

