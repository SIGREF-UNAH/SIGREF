using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Services.Healthcare;

namespace SIGREF.API.Controllers.Healthcare;

[Route("api/[controller]")]
//[ApiController]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class HealthcaresController : ControllerBase
{
    private readonly IHealthcareService _healthcareService;

    public HealthcaresController(IHealthcareService healthcareService)
    {
        _healthcareService = healthcareService;
    }

    // ============================================================
    // LISTAR / FILTRAR
    // ============================================================
    [HttpGet]
   // [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(ResponseDto<PagedResultDto<HealthcareDto>>))]
    public async Task<IActionResult> GetFiltered([FromQuery] HealthcareFilterDto filter)
    {
        var result = await _healthcareService.GetFilteredAsync(filter);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    // OBTENER POR ID
    // ============================================================
    [HttpGet("{id}")]
    //[Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(ResponseDto<HealthcareDto>))]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _healthcareService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    // CREAR
    // ============================================================
    [HttpPost]
   // [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(ResponseDto<HealthcareDto>))]
    public async Task<IActionResult> Create([FromBody] CreateHealthcareDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _healthcareService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    // ACTUALIZAR
    // ============================================================
    [HttpPut("{id}")]
   // [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(ResponseDto<HealthcareDto>))]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateHealthcareDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _healthcareService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================================
    // ELIMINAR
    // ============================================================
    [HttpDelete("{id}")]
   // [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(ResponseDto<bool>))]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _healthcareService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}