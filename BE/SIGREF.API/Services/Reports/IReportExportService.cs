using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;

namespace SIGREF.API.Services.Reports;

public interface IReportExportService
{
    // TODO :
    // =====================================================
    // EXPORTACION (JOB)
    // ===============================================

    /// <summary>
    /// Crea un job de exportación de reporte (PDF / Excel / CSV).
    /// No genera el archivo en el request.
    /// </summary>
    Task<ResponseDto<CreateReportExportResponseDto>> CreateReportExportAsync(
        CreateReportExportRequestDto request);

    /// <summary>
    /// Obtiene el estado actual de un job de exportación.
    /// </summary>
    Task<ResponseDto<ReportExportStatusResponseDto>> GetReportExportStatusAsync(
        Guid jobId);

    /// <summary>
    /// Obtiene el archivo generado por el job de exportación.
    /// Retorna null si el job no está completado.
    /// </summary>
    Task<FileResultDto?> GetReportExportFileAsync(Guid jobId);
}