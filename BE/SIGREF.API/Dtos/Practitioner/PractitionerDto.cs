#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;

namespace SIGREF.API.Dtos.Practitioner;

public class PractitionerDto
{
    public string? Id { get; set; }
    public List<IdentifierDto>? Identifier { get; set; }
    public bool? Active { get; set; }
    public List<HumanNameDto>? Name { get; set; }
    public List<ContactPointDto>? Telecom { get; set; }
    public AdministrativeGender? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public List<PractitionerRoleDto>? Roles { get; set; }
}
