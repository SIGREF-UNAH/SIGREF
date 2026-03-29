
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
    public Guid RequestedByUserId { get; set; } = Guid.Empty;

    /// <summary>
    /// Identificador único del trabajo (Job) generado por el motor de Hangfire.
    /// </summary>
    /// <remarks>
    /// <b>Arquitectura y Persistencia:</b>
    /// Aunque la API de Hangfire expone este ID como <see cref="string"/> para mantener la abstracción del almacenamiento 
    /// (Storage Agnostic), su origen en SQL Server es un contador autoincremental <c>IDENTITY</c>.
    /// 
    /// <b>Consideraciones de Longitud:</b>
    /// - En esquemas estándar de SQL Server, el tipo subyacente es <c>INT</c> (hasta 10 dígitos).
    /// - En implementaciones de alto volumen o esquemas actualizados, se utiliza <c>BIGINT</c> (hasta 20 dígitos).
    /// - Se recomienda definir la columna en tablas de auditoría/históricos como <c>NVARCHAR(50)</c> 
    ///   para absorber cambios futuros hacia GUIDs o UUIDs sin afectar el esquema de negocio.
    /// 
    /// <b>Riesgo de Colisión:</b>
    /// Si el almacenamiento de Hangfire es reseteado o truncado, los IDs podrían volver a comenzar desde 1. 
    /// Se sugiere no utilizar este campo como Llave Primaria única en tablas históricas.
    /// </remarks>
    /// <seealso href="https://github.com/HangfireIO/Hangfire/blob/333bd8eb228402abcee3f261cf32412e844f32c0/src/Hangfire.SqlServer/Install.sql#L82-L89">
    /// Referencia de Esquema Oficial (Hangfire.SqlServer)
    /// </seealso>
    /// <seealso href="https://github.com/hangfire-postgres/Hangfire.PostgreSql/issues/100">
    /// Discusión sobre la transición de INT a BIGINT/UUID
    /// </seealso>
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