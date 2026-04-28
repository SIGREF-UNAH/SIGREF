using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Location;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Location;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class LocationsController(ILocationService locationService) : ControllerBase
{
    // GET: api/locations
    /// <summary>
    /// Obtiene una lista paginada y filtrada de ubicaciones médicas.
    /// </summary>
    /// <param name="filter">Criterios de búsqueda y parámetros de paginación.</param>
    /// <response code="200">Lista de ubicaciones obtenida exitosamente.</response>
    /// <response code="400">Parámetros de búsqueda inválidos.</response>
    [HttpGet]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(PagedResultDto<LocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PagedResultDto<LocationDto>>> Get([FromQuery] LocationFilterDto filter)
    {
        // El servicio ya se encarga de:
        // 1. Validar PageSize.
        // 2. Normalizar paginación.
        // 3. Mapear a DTOs.
        // 4. Manejar excepciones de FHIR vía Mapper.
        var result = await locationService.GetFilteredLocationsAsync(filter);
    
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una ubicación específica por su ID.
    /// </summary>
    /// <param name="id">Identificador de la ubicación.</param>
    /// <response code="200">Ubicación encontrada y devuelta con éxito.</response>
    /// <response code="404">No se encontró ninguna ubicación con el ID proporcionado.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<LocationDto>> GetById(string id) 
    {
        // 1. El servicio ahora recibe un 'string' (estándar FHIR).
        // 2. Si no existe, el servicio lanza NotFoundException, 
        //    el Middleware la atrapa y devuelve el 404 automáticamente.
        var locationDto = await locationService.GetLocationByIdAsync(id);
    
        return Ok(locationDto);
    }

    /// <summary>
    /// Crea una nueva ubicación médica en el servidor FHIR.
    /// </summary>
    /// <param name="dto">Datos de la ubicación a crear.</param>
    /// <response code="201">Ubicación creada exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos o formato incorrecto.</response>
    /// <response code="422">Regla de negocio violada (ej. organización padre inexistente).</response>
    [HttpPost]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto dto)
    {
        var result = await locationService.CreateLocationAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza una ubicación médica existente.
    /// </summary>
    /// <param name="id">Identificador de la ubicación.</param>
    /// <param name="dto">Datos actualizados de la ubicación.</param>
    /// <response code="200">Ubicación actualizada exitosamente.</response>
    /// <response code="404">La ubicación no existe.</response>
    /// <response code="409">Conflicto de concurrencia o de integridad en el servidor FHIR.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<LocationDto>> UpdateLocation(string id, [FromBody] UpdateLocationDto dto)
    {
        var result = await locationService.UpdateLocationAsync(id, dto);

        return Ok(result);
    }

    /// <summary>
    /// Elimina una ubicación física del sistema.
    /// </summary>
    /// <param name="id">Identificador de la ubicación.</param>
    /// <response code="204">Ubicación eliminada con éxito.</response>
    /// <response code="404">La ubicación no existe.</response>
    /// <response code="409">Conflicto: La ubicación tiene registros vinculados que impiden su borrado.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> DeleteLocation(string id)
    {
        await locationService.DeleteLocationAsync(id);

        return NoContent();
    }
}