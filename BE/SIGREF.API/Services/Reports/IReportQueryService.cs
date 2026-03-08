using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Report;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Reports;

public interface IReportQueryService
{
    // <summary>
    /// Obtiene el resumen del reporte (totales y conteos).
    /// Respuesta liviana, pensada para carga rápida en UI.
    /// </summary>
    Task<ResponseDto<ReportSummaryResponseDto>> GetReportSummaryAsync(
        ReportFilterDto filter);

    /// <summary>
    /// Obtiene el detalle del reporte de forma paginada.
    /// Diseñado para tablas en UI (no devuelve miles de filas).
    /// Si necesita USAR miles de filas UTILIZAR EL EXPORT MEDIANTE LOS JOBS
    /// </summary>
    Task<ResponseDto<ReportDetailPageResponseDto>> GetReportDetailPageAsync(
        ReportFilterDto filter);
}