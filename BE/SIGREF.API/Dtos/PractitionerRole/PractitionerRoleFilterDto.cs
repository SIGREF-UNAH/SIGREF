#nullable enable

public class PractitionerRoleFilterDto
{
    public bool? Active { get; set; }
    public string? OrganizationId { get; set; }
    public string? Specialty { get; set; }

    // Paginación
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
