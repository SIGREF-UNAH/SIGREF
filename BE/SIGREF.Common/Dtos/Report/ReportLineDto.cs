namespace SIGREF.Common.Dtos.Report;

/// <summary>
/// Representa una fila/línea del reporte.
/// </summary>
public class ReportLineDto
{
    /// <summary>
    /// Fecha/hora asociada al registro (por ejemplo: fecha de factura/recibo).
    /// </summary>
    public DateTimeOffset TransactionDate { get; set; }

    /// <summary>
    /// Número de recibo/boleta/comprobante.
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cajero/usuario que atendió.
    /// </summary>
    public string CashierName { get; set; } = string.Empty;
    public string CashierIdentity { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del paciente/cliente asociado a la transacción (si aplica).
    /// </summary>
    public string PatientName { get; set; } = string.Empty;
    public string PatientIdentity { get; set; } = string.Empty;
    public string PatientBirthDate { get; set; } = string.Empty;

    /// <summary>
    /// Servicio prestado (consulta, laboratorio, etc.).
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Estado del registro (Pagado, Anulado, Exonerado, etc.).
    /// Idealmente esto debería ser un enum, pero se deja string por compatibilidad.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Monto pagado por el servicio (si aplica).
    /// </summary>
    public decimal AmountPaid { get; set; }
}
