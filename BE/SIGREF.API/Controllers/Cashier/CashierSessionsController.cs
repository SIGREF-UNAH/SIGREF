using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Cashier;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Sesiones de Caja - Gestión de Sesiones")]
public class CashierSessionsController : ControllerBase
{
    private readonly ICashierSessionService _cashierSessionService;

    public CashierSessionsController(ICashierSessionService cashierSessionService)
    {
        _cashierSessionService = cashierSessionService;
    }

    // ============================================================
    //                  ABRIR SESION DE CAJA
    // ============================================================
    [HttpPost("open")]
    [SwaggerOperation(
        OperationId = "CreateSessionOpen",
        Summary = "Abrir sesión de caja",
        Description = "Inicia una nueva sesión de caja para un cajero autenticado.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ResponseDto<CashierSessionMinimalDto>))]
    public async Task<IActionResult> OpenSession([FromBody] CreateCashierSessionDto dto)
    {
        var result = await _cashierSessionService.OpenSessionAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                    CERRAR SESIÓN DE CAJA
    // ============================================================
    [HttpPost("{sessionId:guid}/close")]
    [SwaggerOperation(
        OperationId = "CreateSessionCloseById",
        Summary = "Cerrar sesión de caja",
        Description = "Cierra una sesión de caja activa utilizando su ID y los datos de cierre proporcionados.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<CashierSessionDto>))]
    public async Task<IActionResult> CloseSession(
        Guid sessionId,
        [FromBody] CloseCashierSessionDto dto)
    {
        var result = await _cashierSessionService.CloseSessionAsync(sessionId, dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                SOLICITAR CORRECCIÓN (CAJERO)
    // ============================================================
    [HttpPost("{sessionId:guid}/request-correction")]
    [SwaggerOperation(
        OperationId = "CreateSessionCorrection",
        Summary = "Solicitar corrección de sesión",
        Description = "Permite a un cajero solicitar una corrección para una sesión de caja que ya fue cerrada.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<CashierSessionDto>))]
    public async Task<IActionResult> RequestCorrection(
        Guid sessionId,
        [FromBody] RequestCorrectionDto dto)
    {
        var result = await _cashierSessionService.RequestCorrectionAsync(sessionId, dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //         RESOLVER CORRECCIÓN (ADMIN / AUDITOR)
    // ============================================================
    [HttpPost("{sessionId:guid}/resolve-correction")]
    [SwaggerOperation(
        OperationId = "UpdateSessionResolveCorrection",
        Summary = "Resolver corrección de sesión",
        Description = "Permite a un administrador o auditor evaluar y resolver una solicitud de corrección emitida por un cajero.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<CashierSessionDto>))]
    public async Task<IActionResult> ResolveCorrection(
        Guid sessionId,
        [FromBody] ResolveCorrectionDto dto)
    {
        var result = await _cashierSessionService.ResolveCorrectionAsync(sessionId, dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 LISTAR SESIONES DE CAJA
    // ============================================================
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetSessionList",
        Summary = "Obtener sesiones de caja filtradas",
        Description = "Recupera una lista paginada de las sesiones de caja en el sistema, de acuerdo a los filtros proporcionados.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<PagedResultDto<CashierSessionDto>>))]
    public async Task<IActionResult> GetFiltered([FromQuery] CashierSessionFilterDto filter)
    {
        var result = await _cashierSessionService.GetFilteredSessionsAsync(filter);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 OBTENER SESIÓN POR ID
    // ============================================================
    [HttpGet("{sessionId:guid}")]
    [SwaggerOperation(
        OperationId = "GetSessionById",
        Summary = "Obtener una sesión por su ID",
        Description = "Recupera el detalle completo de una sesión de caja específica a partir de su identificador único.",
        Tags = new[] { "CashierSessions" }
    )]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<CashierSessionDto?>))]
    public async Task<IActionResult> GetById(Guid sessionId)
    {
        var result = await _cashierSessionService.GetByIdAsync(sessionId);
        return StatusCode(result.StatusCode, result);
    }
}
