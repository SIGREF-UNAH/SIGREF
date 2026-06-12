using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.Dashboard;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Dashboard;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Dashboard - Resumen de Estadisticas")]
public class DashboardController(IDashboardReportingService dashboardReportingService) : ControllerBase
{
    [HttpGet("summary")]
    [SwaggerOperation(
        OperationId = "GetDashboardSummary",
        Summary = "Obtiene el Resumen general de estadisticas de ingresos",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetSummaryAsync(filter);
        return Ok(result);
    }

    [HttpGet("service-usage")]
    [SwaggerOperation(
        OperationId = "GetDashboardServiceUsage",
        Summary = "Obtiene el Resumen de servicios usados",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType( typeof(ServiceUsageResultDto) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetServiceUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("package-usage")]
    [SwaggerOperation(
        OperationId = "GetDashboardPackageUsage",
        Summary = "Obtiene el Resumen de paquetes usados",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(PackageUsageResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPackageUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetPackageUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("weekly-income")]
    [SwaggerOperation(
        OperationId = "GetDashboardWeeklyIncome",
        Summary = "Obtiene el Resumen de ingresos semanales",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(List<WeeklyIncomeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklyIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetWeeklyIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("shift-income")]
    [SwaggerOperation(
        OperationId = "GetDashboardShiftIncome",
        Summary = "Obtiene el Resumen de ingresos por turnos",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(List<ShiftIncomeDto>) , StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetShiftIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("location-income")]
    [SwaggerOperation(
        OperationId = "GetDashboardLocationIncome",
        Summary = "Obtiene el Resumen de servicios usados",
        Description = "NA",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(List<LocationIncomeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetLocationIncomeAsync(filter);
        return Ok(result);
    }
}