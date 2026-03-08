#nullable enable

using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

public class PractitionerRoleFilterDto : PagedFilterBase
{
    public bool? Active { get; set; }
    public string? OrganizationId { get; set; }
    public string? Specialty { get; set; }
}
