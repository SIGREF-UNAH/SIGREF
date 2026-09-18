using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;

namespace SIGREF.API.Controllers.Cashier;

/// <summary>Gestiona el ciclo de vida de las sesiones de caja de SIGREF.</summary>
/// <remarks>
///     Dominio SIGREF: registra aperturas, cierres, correcciones y consultas de sesiones. No expone recursos FHIR
///     directamente.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("CashierSessions")]
public class CashierSessionsController : ControllerBase
{
    private readonly ICashierSessionService _cashierSessionService;
    private readonly IUserContextService _userContext;

    public CashierSessionsController(
        ICashierSessionService cashierSessionService,
        IUserContextService userContext)
    {
        _cashierSessionService = cashierSessionService;
        _userContext = userContext;
    }

    // ============================================================
    //              OBTENER SESION ACTIVA DEL CAJERO
    // ============================================================
    [HttpGet("active")]
    [EndpointName("GetActiveSession")]
    [EndpointSummary("Obtener la sesiÃ³n activa del cajero autenticado")]
    [EndpointDescription(
        "Consulta la sesiÃ³n de caja abierta del usuario autenticado sin aceptar un identificador externo.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var userId = _userContext.GetUserId();
        var result = await _cashierSessionService.GetActiveSessionByUserAsync(userId);
        return Ok(result);
    }

    // ============================================================
    //                  ABRIR SESION DE CAJA
    // ============================================================
    [HttpPost("open")]
    [EndpointName("CreateSessionOpen")]
    [EndpointSummary("Abrir sesión de caja")]
    [EndpointDescription("Inicia una nueva sesión de caja para un cajero autenticado.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(typeof(CashierSessionMinimalDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> OpenSession([FromBody] CreateCashierSessionDto dto)
    {
        var result = await _cashierSessionService.OpenSessionAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ============================================================
    //                    CERRAR SESIÓN DE CAJA
    // ============================================================
    [HttpPost("{sessionId:guid}/close")]
    [EndpointName("CreateSessionCloseById")]
    [EndpointSummary("Cerrar sesión de caja")]
    [EndpointDescription("Cierra una sesión de caja activa utilizando su ID y los datos de cierre proporcionados.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseSession(
        Guid sessionId,
        [FromBody] CloseCashierSessionDto dto)
    {
        var result = await _cashierSessionService.CloseSessionAsync(sessionId, dto);
        return Ok(result);
    }

    // ============================================================
    //                SOLICITAR CORRECCIÓN (CAJERO)
    // ============================================================
    [HttpPost("{sessionId:guid}/request-correction")]
    [EndpointName("CreateSessionCorrection")]
    [EndpointSummary("Solicitar corrección de sesión")]
    [EndpointDescription("Permite a un cajero solicitar una corrección para una sesión de caja que ya fue cerrada.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestCorrection(
        Guid sessionId,
        [FromBody] RequestCorrectionDto dto)
    {
        var result = await _cashierSessionService.RequestCorrectionAsync(sessionId, dto);
        return Ok(result);
    }

    // ============================================================
    //         RESOLVER CORRECCIÓN (ADMIN / AUDITOR)
    // ============================================================
    [HttpPost("{sessionId:guid}/resolve-correction")]
    [EndpointName("UpdateSessionResolveCorrection")]
    [EndpointSummary("Resolver corrección de sesión")]
    [EndpointDescription(
        "Permite a un administrador o auditor evaluar y resolver una solicitud de corrección emitida por un cajero.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveCorrection(
        Guid sessionId,
        [FromBody] ResolveCorrectionDto dto)
    {
        var result = await _cashierSessionService.ResolveCorrectionAsync(sessionId, dto);
        return Ok(result);
    }

    // ============================================================
    //                 LISTAR SESIONES DE CAJA
    // ============================================================
    [HttpGet]
    [EndpointName("GetSessionList")]
    [EndpointSummary("Obtener sesiones de caja filtradas")]
    [EndpointDescription(
        "Recupera una lista paginada de las sesiones de caja en el sistema, de acuerdo a los filtros proporcionados.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(PagedResultDto<CashierSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltered([FromQuery] CashierSessionFilterDto filter)
    {
        var result = await _cashierSessionService.GetFilteredSessionsAsync(filter);
        return Ok(result);
    }

    // ============================================================
    //                 OBTENER SESIÓN POR ID
    // ============================================================
    [HttpGet("{sessionId:guid}")]
    [EndpointName("GetSessionById")]
    [EndpointSummary("Obtener una sesión por su ID")]
    [EndpointDescription(
        "Recupera el detalle completo de una sesión de caja específica a partir de su identificador único.")]
    [Tags("CashierSessions")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid sessionId)
    {
        var result = await _cashierSessionService.GetByIdAsync(sessionId);
        return Ok(result);
    }
}
