using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SIGREF.API.Dtos.Income;
using SIGREF.API.Services.Income;
using SIGREF.API.Services.AuditLog;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Controllers.Income;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class IncomeController : ControllerBase
{
    private readonly IIncomeService _incomeService;
    private readonly ILogger<IncomeController> _logger;
    private readonly IAuditLogService _auditLogService;

    public IncomeController(IIncomeService incomeService, ILogger<IncomeController> logger, IAuditLogService auditLogService)
    {
        _incomeService = incomeService;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateIncomeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _incomeService.CreateIncomeAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ingreso");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("counterpart")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCounterpart([FromBody] CreateCounterpartIncomeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _incomeService.CreateCounterpartAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear contrapartida");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var result = await _incomeService.GetIncomeByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Ingreso no encontrado" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ingreso con ID {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _incomeService.GetAllIncomesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los ingresos");
            return BadRequest(new { message = ex.Message });
        }
    }
}