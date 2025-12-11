using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Services.Practitioner;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PractitionerController : ControllerBase
{
    private readonly IPractitionerService _practitionerService;

    public PractitionerController(IPractitionerService practitionerService)
    {
        _practitionerService = practitionerService;
    }

    // GET: api/practitioner
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $" {RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    [Produces(typeof(PagedResultDto<PractitionerDto>))]
    public async Task<IActionResult> GetFiltered([FromQuery] PractitionerFilterDto filter)
    {
        var pagedPractitioners = await _practitionerService.GetFilteredPractitionersAsync(filter);

        var pagedPractitionerDtos = new PagedResultDto<PractitionerDto>
        {
            Items = pagedPractitioners.Items,
            Pagination = pagedPractitioners.Pagination
        };

        return Ok(pagedPractitionerDtos);
    }

    // GET: api/practitioner/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    [Produces<PractitionerDto>()]
    public async Task<IActionResult> GetById(string id)
    {
        var prectitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        if (prectitioner == null)
            return NotFound($"Patient with id '{id}' not found.");

        return Ok(prectitioner);
    }

    // POST: api/practitioner
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $" {RolesConstants.admin} , {RolesConstants.ti}")]
    [Produces<PractitionerDto>()]
    public async Task<IActionResult> CreatePractitioner([FromBody] CreatePractitionerDto createPractitionerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdPractitioner = await _practitionerService.CreatePractitionerAsync(createPractitionerDto);
        return CreatedAtAction(nameof(GetById), new { id = createdPractitioner.Id }, createdPractitioner);
    }

    // PUT: api/practitioner/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    [Produces<PractitionerDto>()]
    public async Task<IActionResult> UpdatePractitioner(string id, [FromBody] UpdatePractitionerDto updatePractitionerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedPractitioner = await _practitionerService.UpdatePractitionerWithDtoAsync(id, updatePractitionerDto);
        if (updatedPractitioner == null)
            return NotFound($"Practitioner with id '{id}' not found.");

        return Ok(updatedPractitioner);
    }

    // DELETE: api/practitioner/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<IActionResult> DeletePractitioner(string id)
    {
        // 1. Verificar que el paciente exista
        var existingPractitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        if (existingPractitioner == null)
            return NotFound($"Patient with id '{id}' not found.");

        // 2. Eliminar
        await _practitionerService.DeletePractitionerAsync(id);

        // 3. Devolver 204 No Content
        return NoContent();
    }
}

