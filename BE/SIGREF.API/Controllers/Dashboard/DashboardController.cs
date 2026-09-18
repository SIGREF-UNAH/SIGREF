using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.Dashboard;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Dashboard;

/// <summary>Proporciona indicadores agregados para los paneles operativos y financieros.</summary>
/// <remarks>Dominio SIGREF: consolida información de negocio para visualización; no representa un recurso FHIR.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Dashboard")]
public class DashboardController(IDashboardReportingService dashboardReportingService) : ControllerBase
{
    [HttpGet("summary")]
    [EndpointName("GetDashboardSummary")]
    [EndpointSummary("Obtiene el Resumen general de estadisticas de ingresos")]
    [EndpointDescription("Devuelve los principales indicadores agregados de ingresos para el periodo y filtros solicitados.")]
    [Tags("Dashboard")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetSummaryAsync(filter);
        return Ok(result);
    }

    [HttpGet("service-usage")]
    [EndpointName("GetDashboardServiceUsage")]
    [EndpointSummary("Obtiene el Resumen de servicios usados")]
    [EndpointDescription("Devuelve el uso agregado de los servicios médicos durante el periodo consultado.")]
    [Tags("Dashboard")]
    [ProducesResponseType( typeof(ServiceUsageResultDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetServiceUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("package-usage")]
    [EndpointName("GetDashboardPackageUsage")]
    [EndpointSummary("Obtiene el Resumen de paquetes usados")]
    [EndpointDescription("Devuelve el uso agregado de los paquetes de servicios durante el periodo consultado.")]
    [Tags("Dashboard")]
    [ProducesResponseType(typeof(PackageUsageResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPackageUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetPackageUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("weekly-income")]
    [EndpointName("GetDashboardWeeklyIncome")]
    [EndpointSummary("Obtiene el Resumen de ingresos semanales")]
    [EndpointDescription("Devuelve la evolución semanal de los ingresos para el periodo y filtros solicitados.")]
    [Tags("Dashboard")]
    [ProducesResponseType(typeof(List<WeeklyIncomeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetWeeklyIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("shift-income")]
    [EndpointName("GetDashboardShiftIncome")]
    [EndpointSummary("Obtiene el Resumen de ingresos por turnos")]
    [EndpointDescription("Devuelve los ingresos agregados por turno de caja.")]
    [Tags("Dashboard")]
    [ProducesResponseType(typeof(List<ShiftIncomeDto>) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetShiftIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("location-income")]
    [EndpointName("GetDashboardLocationIncome")]
    [EndpointSummary("Obtiene el Resumen de servicios usados")]
    [EndpointDescription("Devuelve los ingresos agregados por ubicación física.")]
    [Tags("Dashboard")]
    [ProducesResponseType(typeof(List<LocationIncomeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetLocationIncomeAsync(filter);
        return Ok(result);
    }
}
