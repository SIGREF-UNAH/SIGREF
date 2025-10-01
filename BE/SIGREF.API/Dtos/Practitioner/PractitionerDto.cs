using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Practitioner;
public class PractitionerDto
{
    public string? Id { get; set; }
    public List<IdentifierDto>? Identifier { get; set; }
    public bool? Active { get; set; }
    public List<HumanNameDto>? Name { get; set; } = new();
    public List<ContactPointDto>? Telecom { get; set; } = new();
    public AdministrativeGender? Gender { get; set; } = AdministrativeGender.Unknown;
    public DateTime? BirthDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}

