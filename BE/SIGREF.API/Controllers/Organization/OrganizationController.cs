using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos;
using SIGREF.API.Services.Organization;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers;

/// <summary>Gestiona las organizaciones que participan en la operación sanitaria.</summary>
/// <remarks>FHIR: representa y administra recursos Organization mediante la capa de integración de SIGREF.</remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly ILogger<OrganizationsController> _logger;
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    [HttpGet]
    [EndpointName("GetOrganizationList")]
    [EndpointSummary("Listar organizaciones")]
    [EndpointDescription("Obtiene una lista paginada de recursos Organization aplicando los filtros solicitados.")]
    [Tags("Organizations", "FHIR")]
    [ProducesResponseType(typeof(PagedResultDto<OrganizationDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<IActionResult> GetFilteredOrganizations([FromQuery] OrganizationFilterDto filter)
    {
        var pagedOrganizations = await _organizationService.GetFilteredOrganizationsAsync(filter);

        var pagedOrganizationDtos = new PagedResultDto<OrganizationDto>
        {
            Items = pagedOrganizations.Items,
            Pagination = pagedOrganizations.Pagination
        };

        return Ok(pagedOrganizationDtos);
    }

    [HttpGet("{id}")]
    [EndpointName("GetOrganizationById")]
    [EndpointSummary("Obtener una organización por ID")]
    [EndpointDescription("Recupera el recurso Organization identificado por su ID lógico en FHIR.")]
    [Tags("Organizations", "FHIR")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganizationById(string id)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null) return NotFound($"Organización con ID {id} no encontrada");
            return Ok(organization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo organización con ID {Id}", id);
            return StatusCode(500, $"Ocurrió un error al recuperar la organización con ID {id}");
        }
    }

    [HttpPost]
    [EndpointName("CreateOrganization")]
    [EndpointSummary("Crear una organización")]
    [EndpointDescription("Crea un nuevo recurso Organization con la información institucional proporcionada.")]
    [Tags("Organizations", "FHIR")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> CreateOrganization([FromBody] CreateOrganizationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdOrganization = await _organizationService.CreateOrganizationAsync(createDto);
            return CreatedAtAction(nameof(GetOrganizationById), new { id = createdOrganization.Id },
                createdOrganization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando organización");
            return StatusCode(500, "Ocurrió un error al crear la organización");
        }
    }

    [HttpPut("{id}")]
    [EndpointName("UpdateOrganizationById")]
    [EndpointSummary("Actualizar una organización")]
    [EndpointDescription("Actualiza el recurso Organization indicado por su ID lógico.")]
    [Tags("Organizations", "FHIR")]
    [ProducesResponseType(typeof(OrganizationDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> UpdateOrganization(string id,
        [FromBody] UpdateOrganizationDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedOrganization = await _organizationService.UpdateOrganizationAsync(id, updateDto);
            if (updatedOrganization == null) return NotFound($"Organización con ID {id} no encontrada");
            return Ok(updatedOrganization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando organización con ID {Id}", id);
            return StatusCode(500, $"Ocurrió un error al actualizar la organización con ID {id}");
        }
    }

    [HttpDelete("{id}")]
    [EndpointName("DeleteOrganizationById")]
    [EndpointSummary("Eliminar una organización")]
    [EndpointDescription("Elimina el recurso Organization indicado por su ID lógico.")]
    [Tags("Organizations", "FHIR")]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrganization(string id)
    {
        // El Middleware captura todo y Orval recibe el status code correcto.
        await _organizationService.DeleteOrganizationAsync(id);

        return NoContent();
    }
}
