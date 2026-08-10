using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Services.Reports;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Reporting.Interfaces;

namespace SIGREF.API.Controllers.Reports;


/// <summary>Consulta, genera y descarga reportes operativos de forma síncrona o asíncrona.</summary>
/// <remarks>Dominio SIGREF: utiliza consultas y trabajos en segundo plano; no expone recursos FHIR.</remarks>
[Route("reports/[controller]")]
[ApiController]
[Tags("Reportes - Gestión de Reportes")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class ReportsController : ControllerBase
{
    private readonly IReportQueryService _reportQueryService;
    private readonly IReportQueueService _queue;
    private readonly IReportStorageService _storage;
    private readonly IUserContextService _userContextService;

    public ReportsController(IReportQueryService reportQueryService, IReportQueueService reportQueue, IReportStorageService storage, IUserContextService userContextService)
    {
        _reportQueryService = reportQueryService;
        _queue = reportQueue;
        _storage = storage;
        _userContextService = userContextService;


    }
    /// <summary>
    /// Obtiene el resumen general del reporte (totales y conteos).
    /// Pensado para carga rápida de dashboard.
    /// </summary>
    [HttpGet("summary")]
    [EndpointName("GetReportSummary")]
    [EndpointSummary("Obtener resumen del reporte")]
    [EndpointDescription("Obtiene totales y conteos agregados para cargar rápidamente un dashboard de reportes.")]
    [Tags("SIGREF - Reportes")]
    [ProducesResponseType(typeof(ReportSummaryResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportSummary([FromQuery] ReportFilterDto filter)
    {
        var response = await _reportQueryService.GetReportSummaryAsync(filter);

        return Ok(response);
    }

    /// <summary>
    /// Obtiene el detalle del reporte de forma paginada.
    /// Diseñado para tablas (no exportación masiva).
    /// Para ello utilizar el JOB de Exportacion
    /// </summary>
    [HttpGet("detail")]
    [EndpointName("GetReportDetail")]
    [EndpointSummary("Obtener detalle paginado del reporte")]
    [EndpointDescription("Obtiene el detalle paginado del reporte para su consulta en tablas.")]
    [Tags("SIGREF - Reportes")]
    [ProducesResponseType(typeof(ReportDetailPageResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportDetailPage([FromQuery] ReportFilterDto filter)
    {
        var response = await _reportQueryService.GetReportDetailPageAsync(filter);

        return Ok(response);
    }
    /// <summary>
    /// Encola un nuevo reporte. Respuesta inmediata (202 Accepted).
    /// La UI usa el JobId devuelto para hacer polling al endpoint de status.
    /// </summary>
    [HttpPost("generate")]
    [EndpointName("CreateReportEnqueue")]
    [EndpointSummary("Encolar generación de reporte")]
    [EndpointDescription("Encola la generación del reporte y devuelve inmediatamente el identificador del trabajo para polling.")]
    [Tags("SIGREF - Reportes")]
    [ProducesResponseType(typeof(EnqueueReportResponseDto), StatusCodes.Status200OK)]

    public async Task<IActionResult> Enqueue(
        [FromBody] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var userId =  Guid.NewGuid(); // _userContextService.GetUserId();
        var response = await _queue.EnqueueAsync(filter,userId, cancellationToken);

        // 202 Accepted: solicitud recibida, resultado no disponible aún
        return Accepted(response);
    }

    /// <summary>
    /// Estado del job para polling.
    /// La UI llama a este endpoint cada ~3 segundos hasta Status = "Completed" o "Failed".
    /// </summary>
    [HttpGet("{jobId:guid}/status")]
    [EndpointName("GetReportEnqueueStatus")]
    [EndpointSummary("Consultar estado de generación")]
    [EndpointDescription("Consulta el estado actual de un trabajo de generación de reporte.")]
    [Tags("SIGREF - Reportes")]
    public async Task<IActionResult> GetStatus(Guid jobId, CancellationToken cancellationToken)
    {
        var status = await _queue.GetStatusAsync(jobId, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    /// <summary>
    /// Historial de reportes del usuario autenticado.
    /// </summary>
    [HttpGet("history")]
    [EndpointName("GetReportListHistory")]
    [EndpointSummary("Consultar historial de reportes")]
    [EndpointDescription("Obtiene el historial paginado de reportes del usuario autenticado.")]
    [Tags("SIGREF - Reportes")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();
        var history = await _queue.GetHistoryAsync(userId, page, pageSize, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Descarga el PDF. Solo disponible cuando Status = "Completed".
    /// </summary>
    [HttpGet("{jobId:guid}/download")]
    [EndpointName("GetReportDownload")]
    [EndpointSummary("Descargar reporte generado")]
    [EndpointDescription("Descarga el PDF asociado a un trabajo que haya finalizado correctamente.")]
    [Tags("SIGREF - Reportes")]
    public async Task<IActionResult> Download(Guid jobId, CancellationToken cancellationToken)
    {
        var status = await _queue.GetStatusAsync(jobId, cancellationToken);

        if (status is null)
            return NotFound("Job no encontrado.");

        if (status.Status != "Completed")
            return BadRequest($"El reporte no está listo. Estado actual: {status.Status}");

        var stream = await _storage.OpenAsync(
            Path.Combine("reportes", Path.GetFileName($"{jobId}.pdf")), cancellationToken);

        if (stream is null)
            return NotFound("Archivo no encontrado en el servidor.");

        return File(stream, "application/pdf", $"reporte_{jobId:N}.pdf");
    }
}
