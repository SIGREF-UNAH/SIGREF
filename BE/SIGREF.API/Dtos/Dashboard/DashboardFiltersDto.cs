namespace SIGREF.API.Dtos.Dashboard;

public class DashboardFilterDto
{
    /// <summary>
    /// Fecha inicial del rango.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Fecha final del rango.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Lista de Locations a incluir. Si viene vacía = todos.
    /// </summary>
    public List<string> LocationIds { get; set; } = new();

    /// <summary>
    /// Lista de Módulos (si aplica). Si viene vacía = todos.
    /// </summary>
   /// public List<Guid> ModuleIds { get; set; } = new();

    /// <summary>
    /// Lista de turnos (ShiftIds). Si viene vacía = todos.
    /// </summary>
    public List<Guid> ShiftIds { get; set; } = new();

    /// <summary>
    /// Opcional: filtrar datos de un servicio específico
    /// </summary>
    public Guid? ServiceId { get; set; }
}
