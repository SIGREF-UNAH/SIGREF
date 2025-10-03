using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos;
using SIGREF.API.Services.Organization;

namespace SIGREF.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(IOrganizationService organizationService, ILogger<OrganizationsController> logger)
    {
        _organizationService = organizationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAllOrganizations()
    {
        try
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las organizaciones");
            return StatusCode(500, "Ocurrió un error al recuperar las organizaciones");
        }
    }

    [HttpGet("{id}")]
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