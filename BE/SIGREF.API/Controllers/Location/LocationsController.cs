using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Location;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Location;

/// <summary>Gestiona las ubicaciones físicas de la organización sanitaria.</summary>
/// <remarks>FHIR: representa y administra recursos Location, adaptados a los DTOs y reglas de SIGREF.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Locations")]
public class LocationsController(ILocationService locationService) : ControllerBase
{
    // GET: api/locations
    /// <summary>
    /// Obtiene una lista paginada y filtrada de ubicaciones mÃ©dicas.
    /// </summary>
    /// <param name="filter">Criterios de bÃºsqueda y parÃ¡metros de paginaciÃ³n.</param>
    /// <response code="200">Lista de ubicaciones obtenida exitosamente.</response>
    /// <response code="400">ParÃ¡metros de bÃºsqueda invÃ¡lidos.</response>
    [HttpGet]
    [EndpointName("GetLocationList")]
    [EndpointSummary("Listar ubicaciones físicas")]
    [EndpointDescription("Obtiene una lista paginada de recursos Location filtrada por los criterios solicitados.")]
    [Tags("Locations", "FHIR")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType( typeof(PagedResultDto<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<LocationDto>>> Get([FromQuery] LocationFilterDto filter)
    {
        // El servicio ya se encarga de:
        // 1. Validar PageSize.
        // 2. Normalizar paginaciÃ³n.
        // 3. Mapear a DTOs.
        // 4. Manejar excepciones de FHIR vÃ­a Mapper.
        var result = await locationService.GetFilteredLocationsAsync(filter);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una ubicaciÃ³n especÃ­fica por su ID.
    /// </summary>
    /// <param name="id">Identificador de la ubicaciÃ³n.</param>
    /// <response code="200">UbicaciÃ³n encontrada y devuelta con Ã©xito.</response>
    /// <response code="404">No se encontrÃ³ ninguna ubicaciÃ³n con el ID proporcionado.</response>
    [HttpGet("{id}")]
    [EndpointName("GetLocationById")]
    [EndpointSummary("Obtener una ubicación por ID")]
    [EndpointDescription("Recupera el recurso Location identificado por su ID lógico en FHIR.")]
    [Tags("Locations", "FHIR")]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LocationDto>> GetById(string id)
    {
        // 1. El servicio ahora recibe un 'string' (estÃ¡ndar FHIR).
        // 2. Si no existe, el servicio lanza NotFoundException,
        //    el Middleware la atrapa y devuelve el 404 automÃ¡ticamente.
        var locationDto = await locationService.GetLocationByIdAsync(id);

        return Ok(locationDto);
    }

    /// <summary>
    /// Crea una nueva ubicaciÃ³n mÃ©dica en el servidor FHIR.
    /// </summary>
    /// <param name="dto">Datos de la ubicaciÃ³n a crear.</param>
    /// <response code="201">UbicaciÃ³n creada exitosamente.</response>
    /// <response code="400">Datos de entrada invÃ¡lidos o formato incorrecto.</response>
    /// <response code="422">Regla de negocio violada (ej. organizaciÃ³n padre inexistente).</response>
    [HttpPost]
    [EndpointName("CreateLocation")]
    [EndpointSummary("Crear una ubicación")]
    [EndpointDescription("Crea un nuevo recurso Location con los datos físicos y administrativos proporcionados.")]
    [Tags("Locations", "FHIR")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto dto)
    {
        var result = await locationService.CreateLocationAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza una ubicaciÃ³n mÃ©dica existente.
    /// </summary>
    /// <param name="id">Identificador de la ubicaciÃ³n.</param>
    /// <param name="dto">Datos actualizados de la ubicaciÃ³n.</param>
    /// <response code="200">UbicaciÃ³n actualizada exitosamente.</response>
    /// <response code="404">La ubicaciÃ³n no existe.</response>
    /// <response code="409">Conflicto de concurrencia o de integridad en el servidor FHIR.</response>
    [HttpPut("{id}")]
    [EndpointName("UpdateLocationById")]
    [EndpointSummary("Actualizar una ubicación")]
    [EndpointDescription("Actualiza el recurso Location indicado por su ID lógico.")]
    [Tags("Locations", "FHIR")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LocationDto>> UpdateLocation(string id, [FromBody] UpdateLocationDto dto)
    {
        var result = await locationService.UpdateLocationAsync(id, dto);

        return Ok(result);
    }

    /// <summary>
    /// Elimina una ubicaciÃ³n fÃ­sica del sistema.
    /// </summary>
    /// <param name="id">Identificador de la ubicaciÃ³n.</param>
    /// <response code="204">UbicaciÃ³n eliminada con Ã©xito.</response>
    /// <response code="404">La ubicaciÃ³n no existe.</response>
    /// <response code="409">Conflicto: La ubicaciÃ³n tiene registros vinculados que impiden su borrado.</response>
    [HttpDelete("{id}")]
    [EndpointName("DeleteLocationById")]
    [EndpointSummary("Eliminar una ubicación")]
    [EndpointDescription("Elimina el recurso Location indicado por su ID lógico.")]
    [Tags("Locations", "FHIR")]
    [Authorize(Roles = RolesConstants.admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLocation(string id)
    {
        await locationService.DeleteLocationAsync(id);

        return NoContent();
    }
}
