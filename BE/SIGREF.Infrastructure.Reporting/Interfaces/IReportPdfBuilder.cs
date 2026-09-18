using SIGREF.Common.Dtos.Report;

namespace SIGREF.Infrastructure.Reporting.Interfaces;

public interface IReportPdfBuilder
{
    /// <summary>
    ///     Genera un stream de PDF basado en los datos proporcionados.
    /// </summary>
    /// <param name="data">Stream de líneas del reporte ya enriquecidas.</param>
    /// <param name="hospitalSnapshot">JSON con logos y nombres del hospital.</param>
    /// <param name="meta">Datos Adicionales del Reporte</param>
    Task<Stream> BuildAsync(IAsyncEnumerable<ReportLineDto> data, string hospitalSnapshot, ReportMetaDto meta,
        CancellationToken ct);
}