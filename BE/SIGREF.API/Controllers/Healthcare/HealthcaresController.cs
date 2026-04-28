using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Services.Healthcare;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

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
    /// <summary>
    /// Obtiene una lista paginada de servicios de salud con filtros avanzados.
    /// </summary>
    /// <param name="filter">Criterios de búsqueda (Nombre, Especialidad, Organización, etc.)</param>
    /// <response code="200">Lista de servicios de salud obtenida exitosamente.</response>
    /// <response code="400">Error en los parámetros de búsqueda.</response>
    [HttpGet]
    [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti}")]
    [ProducesResponseType(typeof(PagedResultDto<HealthcareDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<HealthcareDto>>> GetFiltered([FromQuery] HealthcareFilterDto filter)
    {
        var result = await _healthcareService.GetFilteredAsync(filter);
    
        return Ok(result);
    }

    // ============================================================
    // OBTENER POR ID
    // ============================================================
    /// <summary>
    /// Obtiene el detalle de un servicio de salud específico por su ID.
    /// </summary>
    /// <param name="id">Identificador único del HealthcareService.</param>
    /// <response code="200">Servicio de salud encontrado exitosamente.</response>
    /// <response code="404">No se encontró el servicio de salud solicitado.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = $"{RolesConstants.auditor},{RolesConstants.admin},{RolesConstants.ti}")]
    [ProducesResponseType(typeof(HealthcareDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<HealthcareDto>> GetById(string id)
    {
        var healthcareDto = await _healthcareService.GetByIdAsync(id);
        return Ok(healthcareDto);
    }

    // ============================================================
    // CREAR
    // ============================================================
    /// <summary>
    /// Crea un nuevo servicio de salud en FHIR y registra su costo en SIGREF.
    /// </summary>
    /// <param name="dto">Datos del servicio (Nombre, Especialidad, Costo, etc.)</param>
    /// <response code="201">Servicio creado y enriquecido exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos.</response>
    [HttpPost]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(HealthcareDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<HealthcareDto>> Create([FromBody] CreateHealthcareDto dto)
    {
        var result = await _healthcareService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ============================================================
    // ACTUALIZAR
    // ============================================================
    /// <summary>
    /// Actualiza un servicio de salud en FHIR y sincroniza su costo en SIGREF.
    /// </summary>
    /// <param name="id">Identificador único del servicio.</param>
    /// <param name="dto">Datos actualizados (incluyendo el costo opcional).</param>
    /// <response code="200">Servicio actualizado y sincronizado exitosamente.</response>
    /// <response code="404">El servicio no existe en el servidor médico.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(HealthcareDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<HealthcareDto>> Update(string id, [FromBody] UpdateHealthcareDto dto)
    {
        var result = await _healthcareService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // ============================================================
    // ELIMINAR
    // ============================================================
    /// <summary>
    /// Elimina un servicio de salud de FHIR y su registro de costo asociado en SIGREF.
    /// </summary>
    /// <param name="id">Identificador único del servicio.</param>
    /// <response code="204">Servicio eliminado exitosamente de ambos sistemas.</response>
    /// <response code="404">El servicio no existe en el servidor médico.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Delete(string id)
    {
        await _healthcareService.DeleteAsync(id);
        return NoContent();
    }
}