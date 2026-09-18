#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;
using SIGREF.API.Dtos.Common.ValidationAtributes;

namespace SIGREF.API.Dtos.Practitioner;

public class CreatePractitionerDto
{
    public List<IdentifierDto> Identifier { get; set; }

    [Required] public bool? Active { get; set; }

    [Required(ErrorMessage = "Al menos un nombre es obligatorio.")]
    [AtLeastOneNameRequired]
    public List<HumanNameDto>? Name { get; set; } = new();

    public List<ContactPointDto>? Telecom { get; set; } = new();


    public AdministrativeGender? Gender { get; set; }

    public DateTime? BirthDate { get; set; }
}

public class UpdatePractitionerDto : UpdateRequestDto
{
    public List<IdentifierDto>? Identifier { get; set; }

    public bool? Active { get; set; }

    [AtLeastOneNameRequired]
    public List<HumanNameDto>? Name { get; set; }

    public List<ContactPointDto>? Telecom { get; set; }

    public AdministrativeGender? Gender { get; set; }

    public DateTime? BirthDate { get; set; }
}
