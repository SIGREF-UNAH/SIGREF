using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Dtos.Reports;

namespace SIGREF.Infrastructure.Reporting.Interfaces;

public interface IReportDataCollector
{
    /// <summary>
    /// Obtiene las líneas del reporte en forma de Stream continuo (Bajo consumo de RAM).
    /// </summary>
    IAsyncEnumerable<ReportLineDto> StreamReportLinesAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
}