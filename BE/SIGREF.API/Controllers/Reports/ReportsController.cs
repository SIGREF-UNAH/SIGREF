using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Services.Reports;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Reporting.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Reports;


[Route("reports/[controller]")]
[ApiController]
[SwaggerTag("Reportes - Gestión de Reportes")]
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
    [SwaggerOperation(
        OperationId = "GetReportSummary",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
    [ProducesResponseType(typeof(ResponseDto<ReportSummaryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportSummary([FromQuery] ReportFilterDto filter)
    {
        var response = await _reportQueryService.GetReportSummaryAsync(filter);

        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Obtiene el detalle del reporte de forma paginada.
    /// Diseñado para tablas (no exportación masiva).
    /// Para ello utilizar el JOB de Exportacion
    /// </summary>
    [HttpGet("detail")]
    [SwaggerOperation(
        OperationId = "GetReportDetail",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailPageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportDetailPage([FromQuery] ReportFilterDto filter)
    {
        var response = await _reportQueryService.GetReportDetailPageAsync(filter);

        return StatusCode(response.StatusCode, response);
    }
    /// <summary>
    /// Encola un nuevo reporte. Respuesta inmediata (202 Accepted).
    /// La UI usa el JobId devuelto para hacer polling al endpoint de status.
    /// </summary>
    [HttpPost("generate")]
    [SwaggerOperation(
        OperationId = "CreateReportEnqueue",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
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
    [SwaggerOperation(
        OperationId = "GetReportEnqueueStatus",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
    public async Task<IActionResult> GetStatus(Guid jobId, CancellationToken cancellationToken)
    {
        var status = await _queue.GetStatusAsync(jobId, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }
 
    /// <summary>
    /// Historial de reportes del usuario autenticado.
    /// </summary>
    [HttpGet("history")]
    [SwaggerOperation(
        OperationId = "GetReportListHistory",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
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
    [SwaggerOperation(
        OperationId = "GetReportDownload",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Reports" }
    )]
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
