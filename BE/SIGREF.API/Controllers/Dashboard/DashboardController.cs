using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.Dashboard;

namespace SIGREF.API.Controllers.Dashboard;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
public class DashboardController(IDashboardReportingService dashboardReportingService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<DashboardSummaryDto>))]
    public async Task<IActionResult> GetSummary([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetSummaryAsync(filter);
        return Ok(result);
    }

    [HttpGet("service-usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<ServiceUsageResultDto>))]
    public async Task<IActionResult> GetServiceUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetServiceUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("package-usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<PackageUsageResultDto>))]
    public async Task<IActionResult> GetPackageUsage([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetPackageUsageAsync(filter);
        return Ok(result);
    }

    [HttpGet("weekly-income")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<List<WeeklyIncomeDto>>))]
    public async Task<IActionResult> GetWeeklyIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetWeeklyIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("shift-income")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<List<ShiftIncomeDto>>))]
    public async Task<IActionResult> GetShiftIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetShiftIncomeAsync(filter);
        return Ok(result);
    }

    [HttpGet("location-income")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<List<LocationIncomeDto>>))]
    public async Task<IActionResult> GetLocationIncome([FromQuery] DashboardFilterDto filter)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await dashboardReportingService.GetLocationIncomeAsync(filter);
        return Ok(result);
    }
}