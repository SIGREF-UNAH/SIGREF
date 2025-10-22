using Hl7.Fhir.Model.CdsHooks;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.PractitionerRole;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFiltered([FromQuery] PractitionerRoleFilterDto filters)
    {
        var roles = await _prService.GetFilteredAsync(filters);
        return Ok(roles);
    }

    // GET BY ID 
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(string id)
    {
        var role = await _prService.GetByIdAsync(id);
        if (role == null) return NotFound($"PractitionerRole with id '{id}' not found.");
        return Ok(role);
    }

    //CREATE 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _prService.DeleteAsync(id);
        if (!deleted) return NotFound($"PractitionerRole with id '{id}' not found.");

        return NoContent();
    }
}

