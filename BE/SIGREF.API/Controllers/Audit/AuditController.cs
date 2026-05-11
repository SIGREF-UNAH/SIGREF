using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Audit.Dto;
using SIGREF.API.Dtos.Audit;
using SIGREF.API.Services.Audit;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace SIGREF.API.Controllers.Audit;

/// <summary>
/// Controlador para la gestión y consulta de logs de auditoría del sistema.
/// </summary>
/// <remarks>
/// Proporciona endpoints para recuperar registros de auditoría con filtros avanzados,
/// permitiendo trazabilidad completa de acciones en el sistema FHIR.
/// 
/// **Roles requeridos:** Administrador, TI
/// </remarks>
[ApiController]
[Route("api/[controller]")]
//[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Auditoría - Consulta y recuperación de logs de auditoría del sistema")]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }
    //[Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetAuditLogs",
        Summary = "Obtener logs de auditoría paginados",
        Description = "Recupera una lista paginada de logs de auditoría con filtros avanzados para trazabilidad FHIR",
        Tags = new[] { "Audit" }
    )]
    [ProducesResponseType(typeof(PagedResultDto<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] 
        AuditLogFilterDto filter,
        CancellationToken ct = default)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filter);
        
        // Agregar metadata de paginación en headers HTTP
        //Response.Headers.Append("X-Total-Count", result.Pagination.TotalItems.ToString());
        //Response.Headers.Append("X-Page", result.Pagination.CurrentPage.ToString());
        //Response.Headers.Append("X-Page-Size", result.Pagination.PageSize.ToString());
        //Response.Headers.Append("X-Total-Pages", result.Pagination.TotalPages.ToString());
        //Response.Headers.Append("X-Has-Next-Page", (result.Pagination.HasNext).ToString().ToLower());
        //Response.Headers.Append("X-Has-Previous-Page", (result.Pagination.HasPrevious).ToString().ToLower());
        
        return Ok(result);
    }

    [Authorize(Roles = $"{RolesConstants.ti},{RolesConstants.admin}")]
    [HttpGet("{id:length(24)}")]
    [SwaggerOperation(
        OperationId = "GetAuditLogById",
        Summary = "Obtener log de auditoría por ID",
        Description = "Recupera un log de auditoría específico por su identificador único de MongoDB",
        Tags = new[] { "Audit" }
    )]
    [ProducesResponseType(typeof(AuditLog), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogById(
        [FromRoute, SwaggerParameter("ID del log de auditoría (ObjectId MongoDB de 24 caracteres hexadecimales)", Required = true)] 
        string id)
    {
        var auditLog = await _auditLogService.GetAuditLogByIdAsync(id);
        return Ok(auditLog);
    }
}
