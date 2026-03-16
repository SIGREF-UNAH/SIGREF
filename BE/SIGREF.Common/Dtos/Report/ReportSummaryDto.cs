namespace SIGREF.Common.Dtos.Report;

/// <summary>
/// Resumen del reporte: totales y conteos agregados.
/// </summary>
public class ReportSummaryDto
{
    /// <summary>
    /// Cantidad total de transacciones incluidas en el reporte.
    /// </summary>
    public long TotalTransactions { get; set; }

    /// <summary>
    /// Total general recaudado/ingresado.
    /// </summary>
    public decimal TotalCollected { get; set; }

    /// <summary>
    /// Cantidad de servicios exonerados.
    /// </summary>
    public int ExoneratedServicesCount { get; set; }

    /// <summary>
    /// Cantidad de servicios pagados.
    /// </summary>
    public int PaidServicesCount { get; set; }

    /// <summary>
    /// Cantidad de servicios anulados/cancelados.
    /// </summary>
    public int CanceledServicesCount { get; set; }

    /// <summary>
    /// Series ejecutadas/aplicadas durante el periodo (ej: series de facturación).
    /// </summary>
    public List<string> ExecutedSeries { get; set; } = new();
}
