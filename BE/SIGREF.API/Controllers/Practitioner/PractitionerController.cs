using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
public class PractitionerController : ControllerBase
{
    private readonly IPractitionerService _practitionerService;

    public PractitionerController(IPractitionerService practitionerService)
    {
        _practitionerService = practitionerService;
    }


    // GET: api/practitioner
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetFiltered([FromQuery] PractitionerFilterDto filter)
    {
        var practitioners = await _practitionerService.GetFilteredPractitionersAsync(filter);
        return Ok(practitioners);
    }
    
    // GET: api/practitioner/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
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
    [Produces("application/json")]
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
    [Produces("application/json")]
    public async Task<IActionResult> UpdatePractitioner(string id, [FromBody] UpdatePractitionerDto updatePractitionerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingPractitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        if (existingPractitioner == null)
            return NotFound($"Practitioner with id '{id}' not found.");

        // Aplicar actualizaciones usando extension method
        existingPractitioner.ApplyUpdate(updatePractitionerDto);
        var updatePractitioner = await _practitionerService.UpdatePractitionerAsync(id, existingPractitioner);

        // 3. Devolver el resultado
        return Ok(updatePractitioner);
    }

    // DELETE: api/practitioner/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

