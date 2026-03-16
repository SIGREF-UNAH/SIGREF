using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIGREF.Common.Dtos.Reports;
using SIGREF.Common.Types;
using SIGREF.Infrastructure.Persistence;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.Hangfire.Worker.Jobs;

/// <summary>
/// Job que Hangfire ejecuta en el Worker.
///
/// Responsabilidades:
///   1. Actualizar el estado en nuestra BD (Processing → Completed/Failed)
///   2. Orquestar: DataCollector → PdfBuilder → Storage
///
/// Lo que NO hace:
///   - Reintentos: los maneja Hangfire con [AutomaticRetry]
///   - Logs de error detallados: los guarda Hangfire en su BD
/// </summary>
public class GenerateReportJob : IGenerateReportJob
{
    private readonly SIGREFContext             _context;
    private readonly IReportDataCollector      _dataCollector;
    private readonly IReportPdfBuilder         _pdfBuilder;
    private readonly IReportStorageService     _storage;
    private readonly ILogger<GenerateReportJob> _logger;

    public GenerateReportJob(
        SIGREFContext context,
        IReportDataCollector dataCollector,
        IReportPdfBuilder pdfBuilder,
        IReportStorageService storage,
        ILogger<GenerateReportJob> logger)
    {
        _context       = context;
        _dataCollector = dataCollector;
        _pdfBuilder    = pdfBuilder;
        _storage       = storage;
        _logger        = logger;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 300 })]
    public async Task ExecuteAsync(
        Guid historyId,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar la entidad en la base de datos
        var history = await _context.ReportHistory
            .FirstOrDefaultAsync(x => x.Id == historyId, cancellationToken);

        if (history == null)
        {
            _logger.LogError("[Job {JobId}] No se encontró el registro de historial {HistoryId}", historyId, historyId);
            return;
        }

        try
        {
            _logger.LogInformation("[Job] Iniciando reporte: {Type} para el usuario {User}", history.ReportType, history.CreatedById);

            // 2. Actualizar estado a Processing
            history.Status = ReportStatus.Processing;
            history.Progress = 5; // Iniciando
            await _context.SaveChangesAsync(cancellationToken);
            
            // 3. TODO: Obtener Filtros
            // Aquí deberías deserializar los filtros si los guardaste en la entidad
             var filters = JsonSerializer.Deserialize<ReportFilterDto>(history.FilterJson?? "");

            // 4. Streaming de Datos (DataCollector)
            
            var dataStream = _dataCollector.StreamReportLinesAsync(filters, cancellationToken);
            // 5. TODO: Generación del PDF con QuestPDF
            using var pdfStream = await _pdfBuilder.BuildAsync(dataStream, history.HospitalPropertiesSnapshot, cancellationToken);

            var downloadUrl = await _storage.SaveAsync(historyId, pdfStream,   cancellationToken);
            history.DownloadUrl = downloadUrl;
            
            
            await _context.SaveChangesAsync(cancellationToken);
            // 6. Guardar el archivo físico
            // var fileName = $"{history.Id}.pdf";
            // var downloadPath = await _storage.SaveAsync(history.Id, pdfStream, cancellationToken);
            // 7. Finalizar
            history.Status = ReportStatus.Completed;
            history.Progress = 100;
            // history.DownloadUrl = downloadPath;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[Job] Reporte {HistoryId} completado exitosamente", historyId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[Job] El reporte {HistoryId} fue cancelated/abortado", historyId);
            throw; // Re-lanzamos para que Hangfire sepa que se canceló
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Job] Error fatal generando reporte {HistoryId}", historyId);
            
            // Guardamos el error para que el usuario sepa qué pasó
            history.Status = ReportStatus.Failed;
            history.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(CancellationToken.None); // Usamos None para asegurar que se guarde el error aunque el token esté cancelado
            
            throw; // Re-lanzamos para que aparezca en el Dashboard de Hangfire
        }
    }
}