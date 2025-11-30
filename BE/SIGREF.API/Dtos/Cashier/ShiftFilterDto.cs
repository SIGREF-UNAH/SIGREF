using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Cashier;

public class ShiftFilterDto : PagedFilterBase
{
    public string? Name { get; set; }           // Nombre del turno
    public bool? IsActive { get; set; }
    public Guid? LocationId { get; set; }   // ubicacion 
    public string? LocationName { get; set; }   // Nombre desde FHIR
}