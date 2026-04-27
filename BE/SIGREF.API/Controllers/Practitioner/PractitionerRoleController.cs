using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PractitionerRoleController : ControllerBase
{
    private readonly IPractitionerRoleService _prService;

    public PractitionerRoleController(IPractitionerRoleService prService)
    {
        _prService = prService;
    }

    // GET ALL
    [HttpGet]
    // Éxito: Especificamos el DTO de paginación con su tipo genérico
    [ProducesResponseType(typeof(PagedResultDto<PractitionerRoleDto>), StatusCodes.Status200OK)]
    // Errores estándar con ProblemDetails
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PagedResultDto<PractitionerRoleDto>>> GetFiltered([FromQuery] PractitionerRoleFilterDto filters)
    {
        var pagedRoles = await _prService.GetFilteredAsync(filters);
        return Ok(pagedRoles);
    }

    // GET BY ID 
    [HttpGet("{id}")]
    // Éxito: Retorna un solo objeto DTO
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    // Errores:
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)] // Vital para un Get por ID
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> GetById(string id)
    {
        var role = await _prService.GetByIdAsync(id);
        return Ok(role);
    }

    // GET BY PRACTITIONER ID 
    [HttpGet("practitioner/{id}")]
    // Especificamos que devuelve una colección (IEnumerable o List)
    [ProducesResponseType(typeof(IEnumerable<PractitionerRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<IEnumerable<PractitionerRoleDto>>> GetByPractitionerId(string id)
    {
        var roles = await _prService.GetByPractitionerIdAsync(id);
        return Ok(roles);
    }

    //CREATE 
    [HttpPost]
    // Éxito: Orval generará una función que retorna PractitionerRoleDto
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status201Created)]
    // Errores: Orval mapeará esto a un objeto de error (ProblemDetails)
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> Create([FromBody] CreatePractitionerRoleDto dto)
    {
        var result = await _prService.CreateAsync(dto);
        // Usamos CreatedAtAction para el estándar REST 201
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // UPDATE
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    // Errores: Orval mapeará esto a un objeto de error (ProblemDetails)
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePractitionerRoleDto dto)
    {
        var result = await _prService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE 
    [HttpDelete("{id}")]
    // Éxito: 204 No Content es el estándar de oro para borrados exitosos
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    // Errores:
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _prService.DeleteAsync(id);
        return NoContent();
    }
}

