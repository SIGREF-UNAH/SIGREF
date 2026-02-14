using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Report;
using SIGREF.API.Services.Reports;

namespace SIGREF.API.Controllers.Reports;


[Route("reports/[controller]")]
[ApiController]
//[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
public class ReportsController : ControllerBase
{
    private readonly IReportQueryService _reportQueryService;

    public ReportsController(IReportQueryService reportQueryService)
    {
        _reportQueryService = reportQueryService;
    }
    /// <summary>
    /// Obtiene el resumen general del reporte (totales y conteos).
    /// Pensado para carga rápida de dashboard.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ResponseDto<ReportSummaryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<ReportSummaryResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(typeof(ResponseDto<ReportDetailPageResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto<ReportDetailPageResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReportDetailPage([FromQuery] ReportFilterDto filter)
    {
        var response = await _reportQueryService.GetReportDetailPageAsync(filter);

        return StatusCode(response.StatusCode, response);
    }
    
}
