#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Common.ValidationAtributes;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Practitioner;


public class CreatePractitionerDto
{

    public List<IdentifierDto> Identifier { get; set; }

    [Required]
    public bool? Active { get; set; }

    [Required(ErrorMessage = "Al menos un nombre es obligatorio.")]
    [AtLeastOneNameRequired]
    public List<HumanNameDto>? Name { get; set; } = new();
    public List<ContactPointDto>? Telecom { get; set; } = new();
    public AdministrativeGender? Gender { get; set; } = AdministrativeGender.Unknown;
    public DateTime? BirthDate { get; set; }
}

public class UpdatePractitionerDto
{
    public List<IdentifierDto> Identifier { get; set; }
    [Required]
    public bool? Active { get; set; }
    [Required(ErrorMessage = "Al menos un nombre es obligatorio.")]
    [AtLeastOneNameRequired]
    public List<HumanNameDto>? Name { get; set; } = new();
    public List<ContactPointDto>? Telecom { get; set; } = new();
    public AdministrativeGender? Gender { get; set; } = AdministrativeGender.Unknown;
    public DateTime? BirthDate { get; set; }

}