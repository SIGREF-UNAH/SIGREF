using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Cashier;
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
    // ===========================================

    /// <summary>
    /// Abre una nueva sesión de caja para el cajero.
    /// </summary>
    [HttpPost("open")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OpenSession([FromBody] CreateCashierSessionDto dto)
    {
        var result = await _cashierSessionService.OpenSessionAsync(dto);

        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                   CERRAR SESIÓN DE CAJA
    // ============================================================

    /// <summary>
    /// Cierra una sesión de caja e incluye el monto declarado.
    /// </summary>
    [HttpPost("{sessionId:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloseSession(Guid sessionId, [FromBody] CloseCashierSessionDto dto)
    {
        var result = await _cashierSessionService.CloseSessionAsync(sessionId, dto);

        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 SOLICITAR CORRECCION (CAJERO)
    // ============================================================

    /// <summary>
    /// El cajero solicita una corrección del arqueo , caja cerro pero inconsistencias.
    /// </summary>
    [HttpPost("{sessionId:guid}/request-correction")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestCorrection(Guid sessionId, [FromBody] RequestCorrectionDto dto)
    {
        var result = await _cashierSessionService.RequestCorrectionAsync(sessionId, dto);

        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //               RESOLVER CORRECCIÓN (ADMIN / AUDITOR)
    // ============================================================

    /// <summary>
    /// Admin o auditor revisan y resuelven la corrección.
    /// TODO VERIFICA ARCHIVO DE SERVICIO 
    /// </summary>
    [HttpPost("{sessionId:guid}/resolve-correction")]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveCorrection(Guid sessionId, [FromBody] ResolveCorrectionDto dto)
    {
        var result = await _cashierSessionService.ResolveCorrectionAsync(sessionId, dto);

        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 LISTAR SESIONES DE CAJA
    // ============================================================

    /// <summary>
    /// Obtiene todas las sesiones de caja filtradas y paginadas.
    /// Admin/Auditor ven todas, cajero solo las suyas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltered([FromQuery] CashierSessionFilterDto filter)
    {
        var result = await _cashierSessionService.GetFilteredSessionsAsync(filter);

        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    //                 OBTENER SESIÓN POR ID
    // ============================================================

    /// <summary>
    /// Obtiene una sesión de caja por su ID.
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid sessionId)
    {
        var result = await _cashierSessionService.GetByIdAsync(sessionId);

        return StatusCode(result.StatusCode, result);
    }
}