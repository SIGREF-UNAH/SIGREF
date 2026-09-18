using System.ComponentModel.DataAnnotations;
using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos;

public class UpdateOrganizationDto : UpdateRequestDto
{
    public List<IdentifierDto> Identifier { get; set; }

    public bool? Active { get; set; }

    public List<CodeableConceptDto> Type { get; set; }

    public string Name { get; set; }

    public List<string> Alias { get; set; }

    public string Description { get; set; }

    public List<ExtendedContactDetailDto> Contact { get; set; }

    public ReferenceDto PartOf { get; set; }

    public List<ReferenceDto> Endpoint { get; set; }
}
