using System.ComponentModel.DataAnnotations;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos;

public class CreateOrganizationDto
{
    public List<IdentifierDto>? Identifier { get; set; }
    
    public bool? Active { get; set; } = true;
    
    public List<CodeableConceptDto>? Type { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public List<string>? Alias { get; set; }
    
    public string? Description { get; set; }
    
    public List<ExtendedContactDetailDto>? Contact { get; set; }
    
    public ReferenceDto? PartOf { get; set; }
    
    public List<ReferenceDto>? Endpoint { get; set; }
}