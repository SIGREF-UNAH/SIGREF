namespace SIGREF.API.Dtos.Cashier;

/// <summary>
/// Data Transfer Object (DTO) para representar una sesión de caja (CashierSession).
/// </summary>
public class CashierSessionDto
{
    /// <summary>
    /// Identificador único de la sesión.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Identificador del usuario que abrió la sesión.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Nombre del usuario que abrió la sesión.
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// Identificador del turno (Shift) al que pertenece la sesión.
    /// </summary>
    public Guid ShiftId { get; set; }
    /// <summary>
    /// Fecha y hora en que se abrió la sesión.
    /// </summary>
    public DateTime OpenAt { get; set; }
    /// <summary>
    /// Fecha y hora en que se cerró la sesión. Puede ser nulo si está abierta.
    /// </summary>
    public DateTime? ClosedAt { get; set; }
    /// <summary>
    /// Monto declarado por el usuario al cerrar la caja.
    /// </summary>
    public decimal? DeclaredAmount { get; set; }
    /// <summary>
    /// Monto calculado por el sistema al cerrar la caja.
    /// </summary>
    public decimal? SystemAmount { get; set; }
    /// <summary>
    /// Diferencia entre el monto declarado y el monto del sistema (DeclaredAmount - SystemAmount).
    /// </summary>
    public decimal? Difference { get; set; }
    /// <summary>
    /// Indica si la sesión se encuentra actualmente abierta.
    /// </summary>
    public bool IsOpen { get; set; }
    /// <summary>
    /// Indica si el cierre de la sesión fue correcto (diferencia cercana a cero o corregida).
    /// </summary>
    public bool IsClosedCorrectly { get; set; } 
    /// <summary>
    /// Notas adicionales sobre la sesión o su corrección.
    /// </summary>
    public string? Notes { get; set; }
    /// <summary>
    /// Fecha y hora en que se aplicó una corrección al cierre de la sesión.
    /// </summary>
    public DateTime? CorrectionClosure { get; set; }
}