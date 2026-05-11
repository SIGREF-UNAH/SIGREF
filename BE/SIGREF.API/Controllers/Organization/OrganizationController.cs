using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Organization;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Organizaciones - Gestion de Organizaciones")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    [HttpGet()]
    [SwaggerOperation(
        OperationId = "GetOrganizationList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Organizations" }
    )]
    [ProducesResponseType( typeof(PagedResultDto<OrganizationDto>), StatusCodes.Status200OK)]
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
    [SwaggerOperation(
        OperationId = "GetOrganizationById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Organizations" }
    )]
    [ProducesResponseType( typeof(OrganizationDto) , StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganizationById(string id)
    {
        try
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return NotFound($"Organización con ID {id} no encontrada");
            }
            return Ok(organization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo organización con ID {Id}", id);
            return StatusCode(500, $"Ocurrió un error al recuperar la organización con ID {id}");
        }
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateOrganization",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Organizations" }
    )]
    [ProducesResponseType( typeof(OrganizationDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> CreateOrganization([FromBody] CreateOrganizationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOrganization = await _organizationService.CreateOrganizationAsync(createDto);
            return CreatedAtAction(nameof(GetOrganizationById), new { id = createdOrganization.Id }, createdOrganization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando organización");
            return StatusCode(500, "Ocurrió un error al crear la organización");
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        OperationId = "UpdateOrganizationById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Organizations" }
    )]
    [ProducesResponseType( typeof(OrganizationDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<ActionResult<OrganizationDto>> UpdateOrganization(string id, [FromBody] UpdateOrganizationDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedOrganization = await _organizationService.UpdateOrganizationAsync(id, updateDto);
            if (updatedOrganization == null)
            {
                return NotFound($"Organización con ID {id} no encontrada");
            }
            return Ok(updatedOrganization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando organización con ID {Id}", id);
            return StatusCode(500, $"Ocurrió un error al actualizar la organización con ID {id}");
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        OperationId = "DeleteOrganizationById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Organizations" }
    )]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrganization(string id)
    {
        // El Middleware captura todo y Orval recibe el status code correcto.
        await _organizationService.DeleteOrganizationAsync(id);
    
        return NoContent();
    }
}