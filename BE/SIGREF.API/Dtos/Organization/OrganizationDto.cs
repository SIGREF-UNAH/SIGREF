using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos;
public class OrganizationDto
{
    public string Id { get; set; }
    public List<IdentifierDto> Identifier { get; set; } = new();
    public bool Active { get; set; } = true;
    public List<CodeableConcept> Types { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public List<string> Alias { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public List<ExtendedContactDetailDto> Contact { get; set; } = new();
    public ReferenceDto PartOf { get; set; }
    public List<ReferenceDto> Endpoint { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
