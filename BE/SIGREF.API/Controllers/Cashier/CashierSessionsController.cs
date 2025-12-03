using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Cashier;

namespace SIGREF.API.Controllers.Cashier;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
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
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.cashier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<CashierSessionDto?>))]
    public async Task<IActionResult> GetById(Guid sessionId)
    {
        var result = await _cashierSessionService.GetByIdAsync(sessionId);
        return StatusCode(result.StatusCode, result);
    }
}
