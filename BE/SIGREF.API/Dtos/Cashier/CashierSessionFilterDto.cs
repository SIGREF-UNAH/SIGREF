using SIGREF.API.Dtos.Common;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Dtos.Cashier;

/// <summary>
/// Filtros aplicables a la consulta de sesiones de caja (CashierSession).
/// Hereda de PagedFilterBase para incluir paginación.
/// </summary>
public class CashierSessionFilterDto : PagedFilterBase
{
    /// <summary>
    /// Filtra por estado de apertura. Null para todos.
    /// </summary>
    public bool? IsOpen { get; set; }          // null = todos
    /// <summary>
    /// Filtra por estado de corrección del cierre. Null para todos.
    /// </summary>
    public bool? IsClosedCorrectly { get; set; } // null = todos
    /// <summary>
    /// Filtro de fecha de inicio (inclusivo).
    /// </summary>
    public DateTime? FromDate { get; set; }
    /// <summary>
    /// Filtro de fecha de fin (inclusivo).
    /// </summary>
    public DateTime? ToDate { get; set; }
    /// <summary>
    /// Filtra por el identificador del turno (ShiftId).
    /// </summary>
    public Guid? ShiftId { get; set; }
}