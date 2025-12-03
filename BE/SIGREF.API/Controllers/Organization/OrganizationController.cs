using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Services.Organization;

namespace SIGREF.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    [Produces(typeof(PagedResultDto<OrganizationDto>))]
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
    [Produces<OrganizationDto>()]
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
    [Produces<OrganizationDto>()]
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
    [Produces<OrganizationDto>()]
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
    [Authorize(Roles = $"{RolesConstants.admin}  , {RolesConstants.ti}")]
    public async Task<ActionResult> DeleteOrganization(string id)
    {
        try
        {
            var result = await _organizationService.DeleteOrganizationAsync(id);
            if (!result)
            {
                return NotFound($"Organización con ID {id} no encontrada");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando organización con ID {Id}", id);
            return StatusCode(500, $"Ocurrió un error al eliminar la organización con ID {id}");
        }
    }
}