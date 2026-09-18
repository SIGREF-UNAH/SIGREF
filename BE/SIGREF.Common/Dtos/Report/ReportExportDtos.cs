using SIGREF.Common.Types;

namespace SIGREF.Common.Dtos.Report;

/// <summary>
///     Respuesta inmediata al crear el job (no devuelve el archivo).
/// </summary>
public class CreateReportExportResponseDto
{
    public Guid JobId { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
///     Request para solicitar una exportación (crea un job).
/// </summary>
public class CreateReportExportRequestDto
{
    public string ReportType { get; set; } = string.Empty; // Ej: "income", "services", etc.
    public ReportExportFormat Format { get; set; }

    /// <summary>
    ///     Rango de fechas obligatorio para evitar reportes gigantes sin control.
    /// </summary>
    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    // Filtros típicos (ajusta a tu dominio):
    public Guid? LocationId { get; set; }
    public Guid? ShiftId { get; set; }
    public Guid? CashierUserId { get; set; }
    public string? Status { get; set; } // o enum si ya lo tienes definido
}

/// <summary>
///     Respuesta para consultar el estado del job.
/// </summary>
public class ReportExportStatusResponseDto
{
    public Guid JobId { get; set; }
    public ReportStatus Status { get; set; }

    /// <summary>
    ///     Progreso estimado 0..100 (opcional, pero útil).
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    ///     Si falló, aquí va el error amigable.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    ///     Metadata del archivo cuando está Completed.
    ///     La descarga real se hace por endpoint de file.
    /// </summary>
    public ReportExportFileDto? File { get; set; }
}

/// <summary>
///     Información del archivo exportado (cuando está listo).
/// </summary>
public class ReportExportFileDto
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public long RowCount { get; set; }

    /// <summary>
    ///     Si usas storage con URL firmada puedes devolverla aquí.
    ///     Si no, omítelo y usa un endpoint /file.
    /// </summary>
    public string? DownloadUrl { get; set; }
}