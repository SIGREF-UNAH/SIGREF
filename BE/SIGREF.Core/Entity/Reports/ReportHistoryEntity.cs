
using SIGREF.Common.Types;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Reports;

/// <summary>
/// Representa el historial y estado de generación de reportes pesados (50k+ registros).
/// Centraliza la trazabilidad entre la solicitud del usuario y el procesamiento en el Hangfire Worker.
/// </summary>
public class ReportHistoryEntity : BaseEntity
{
    /// <summary>Tipo de reporte (Ej: "FacturacionGlobal", "CierreCaja"). Determina la lógica que ejecutará el Worker.</summary>
    public string ReportType { get; set; } = null!;

    /// <summary>ID del usuario que solicitó el reporte para efectos de visualización/auditoría.</summary>
    public string RequestedByUserId { get; set; } = string.Empty;

    /// <summary>Identificador único del Job en Hangfire. Permite monitorear o cancelar la tarea técnica.</summary>
    public string? HangfireJobId { get; set; }

    /// <summary>Estado actual del reporte (Pending, Processing, Completed, Failed).</summary>
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    /// <summary>Porcentaje de avance (0-100). Actualizado por el Worker durante el streaming de datos.</summary>
    public int Progress { get; set; } = 0;

    /// <summary>Ruta o URL de descarga del PDF generado una vez finalizado el proceso.</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>Texto legible del período para mostrar en el historial. Ej: "01/01/2025 – 31/03/2025"</summary>
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>Snapshot de la consulta SQL ejecutada. Crucial para auditoría y entender qué datos se procesaron exactamente.</summary>
    public string SqlQuery { get; set; } = null!;

    /// <summary>Copia literal (JSON) de los datos del hospital al momento del reporte (Director, Logo, etc.). Evita cambios históricos.</summary>
    public string HospitalPropertiesSnapshot { get; set; } = null!;

    /// <summary>En caso de error, almacena la excepción o el motivo del fallo para soporte técnico.</summary>
    public string? ErrorMessage { get; set; }
    
    public string? FilterJson { get; set; }
}