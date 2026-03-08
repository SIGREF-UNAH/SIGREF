using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    [Produces(typeof(PagedResultDto<PractitionerRoleDto>))]
    public async Task<IActionResult> GetFiltered([FromQuery] PractitionerRoleFilterDto filters)
    {
        var pagedRoles = await _prService.GetFilteredAsync(filters);

        var pagedRoleDtos = new PagedResultDto<PractitionerRoleDto>
        {
            Items = pagedRoles.Items,
            Pagination = pagedRoles.Pagination
        };

        return Ok(pagedRoleDtos);
    }

    // GET BY ID 
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> GetById(string id)
    {
        var role = await _prService.GetByIdAsync(id);
        if (role == null) return NotFound($"PractitionerRole with id '{id}' not found.");
        return Ok(role);
    }

    // GET BY PRACTITIONER ID 
    [HttpGet("practitioner/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces<PractitionerRoleDto>()]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<IActionResult> GetByPractitionerId(string id)
    {
        var roles = await _prService.GetByPractitionerIdAsync(id);
        if (roles == null) return NotFound($"Practitioner with id '{id}' not found.");
        return Ok(roles);
    }

    //CREATE 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> Create([FromBody] CreatePractitionerRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _prService.CreateAsync(dto);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "DUPLICATE_IDENTIFIER" or "DUPLICATE_ACTIVE_ROLE" =>
                    Conflict(result.ErrorMessage),
                "INVALID_CODE" =>
                    BadRequest(result.ErrorMessage),
                _ =>
                    StatusCode(500, result.ErrorMessage)
            };
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    // UPDATE
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePractitionerRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _prService.UpdateAsync(id, dto);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(result.ErrorMessage),
                "DUPLICATE_ACTIVE_ROLE" or "DUPLICATE_IDENTIFIER" => Conflict(result.ErrorMessage),
                "INVALID_CODE" => BadRequest(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage)
            };
        }

        return Ok(result.Data);
    }

    // DELETE 
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _prService.DeleteAsync(id);
        if (!deleted) return NotFound($"PractitionerRole with id '{id}' not found.");

        return NoContent();
    }
}

