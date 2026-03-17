using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Audit.Models;
using SIGREF.API.Audit.Services;
using SIGREF.API.Dtos.Audit;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Audit;

[Route("api/[controller]")]
[ApiController]
public class AuditController(IAuditService auditService) : ControllerBase
{

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Produces<ResponseDto<PagedResultDto<AuditLogDto>>>()]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogQueryDto query)
    {
        // Validar rango de fechas
        if (!query.IsValidDateRange())
        {
            return BadRequest(new { message = "La fecha inicial no puede ser mayor a la fecha final" });
        }

        var result = await auditService.GetAuditLogsAsync(query);
        return Ok(result);
    }


    [HttpDelete("test/clear")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RolesConstants.ti},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearAllLogs()
    {
        await auditService.ClearAllLogsAsync();
        return Ok(new { message = "Todos los logs han sido eliminados" });
    }
}
