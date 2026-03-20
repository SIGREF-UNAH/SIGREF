namespace SIGREF.API.Dtos.Cashier;

/// <summary>
/// DTO para la creación de una nueva sesión de caja.
/// </summary>
public class CreateCashierSessionDto
{
    //public Guid UserId { get; set; } // El UserId se obtiene del contexto de seguridad, no se pasa explícitamente.
    /// <summary>
    /// Identificador del turno (Shift) al que se asocia esta sesión.
    /// </summary>
    public Guid ShiftId { get; set; }
}

/// <summary>
/// DTO para el proceso de cierre de una sesión de caja.
/// </summary>
public class CloseCashierSessionDto
{
    /// <summary>
    /// Monto total declarado por el cajero al cerrar la caja.
    /// </summary>
    public decimal DeclaredAmount { get; set; }
}

/// <summary>
/// DTO para solicitar una corrección en el cierre de una sesión de caja.
/// </summary>
public class RequestCorrectionDto
{
    /// <summary>
    /// Notas explicativas del usuario solicitando la corrección.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resolver una solicitud de corrección en el cierre de una sesión de caja.
/// </summary>
public class ResolveCorrectionDto
{
    //public bool MarkAsCorrected { get; set; } // Determinar si se marca como corregido o no.
    /// <summary>
    /// Notas del administrador o persona que resuelve la corrección.
    /// </summary>
    public string? AdminNotes { get; set; }
}

/// <summary>
/// DTO mínimo para identificar una sesión de caja, útil para listados rápidos.
/// </summary>
public class CashierSessionMinimalDto
{
    /// <summary>
    /// Identificador único de la sesión.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Fecha y hora de apertura de la sesión.
    /// </summary>
    public DateTime OpenAt { get; set; }
}