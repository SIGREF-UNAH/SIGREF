using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Dtos.Reports;

namespace SIGREF.Infrastructure.Reporting.Interfaces;

public interface IReportQueueService
{
    /// <summary>Crea el registro en BD y encola el job en Hangfire.</summary>
    Task<EnqueueReportResponseDto> EnqueueAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);
 
    /// <summary>Estado puntual de un job. Usado para polling desde la UI.</summary>
    Task<ReportJobStatusDto?> GetStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
 
    /// <summary>Historial de reportes solicitados por un usuario.</summary>
    Task<IReadOnlyList<ReportJobStatusDto>> GetHistoryAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}