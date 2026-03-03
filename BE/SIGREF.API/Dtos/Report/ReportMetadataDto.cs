namespace SIGREF.API.Dtos.Report;

/// <summary>
/// Metadatos de auditoría / generación del reporte.
/// </summary>
public class ReportMetadataDto
{
    /// <summary>
    /// Fecha/hora en la que se generó el reporte.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Usuario que generó el reporte.
    /// </summary>
    public string GeneratedByUserName { get; set; } = string.Empty;

    /// <summary>
    /// Rol del usuario que generó el reporte (ej: Admin, Cajero, Auditor).
    /// </summary>
    public List<string> GeneratedByRoleName { get; set; } = [];
}
