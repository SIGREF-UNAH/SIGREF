using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions;
using SIGREF.API.Services;

namespace SIGREF.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthcaresController(HealthcareService healthcareService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces<IEnumerable<HealthcareDto>>()]
        public async Task<IActionResult> GetAll()
        {
            var healthcares = await healthcareService.GetAllHealthcaresAsync();
            var healthcareDtos = healthcares.Select(healthcare => healthcare.ToDto());

            return Ok(healthcareDtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces<HealthcareDto>()]
        public async Task<IActionResult> GetById(string id)
        {
            var healthcare = await healthcareService.GetHealthcareByIdAsync(id);
            if (healthcare == null) return NotFound($"Healthcare with id '{id}' not found.");

            var healthcareDto = healthcare.ToDto();

            return Ok(healthcareDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces<HealthcareDto>()]
        public async Task<IActionResult> Create([FromBody] CreateHealthcareDto createHealthcareDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var healthcare = createHealthcareDto.ToFhirHealthcare();

            var createdHealthcare = await healthcareService.CreateHealthcareAsync(healthcare);
            var createdHealthcareDto = createdHealthcare.ToDto();

            // BUG: No puedo retornar el recurso creado, por respuesta tardía del servicio de hapi-fhir
            // return CreatedAtAction(nameof(GetById), new { id = createdHealthcare.Id }, createdHealthcare);

            return Ok(createdHealthcareDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces<HealthcareDto>()]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateHealthcareDto updateHealthcareDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingHealthcare = await healthcareService.GetHealthcareByIdAsync(id);
            if (existingHealthcare == null) return NotFound($"Healthcare with id '{id}' not found.");

            existingHealthcare.ApplyUpdate(updateHealthcareDto);

            await healthcareService.UpdateHealthcareAsync(existingHealthcare);

            var updatedHealthcare = await healthcareService.GetHealthcareByIdAsync(id);

            return Ok(updatedHealthcare.ToDto());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var healthcare = await healthcareService.GetHealthcareByIdAsync(id);
            if (healthcare == null) return NotFound($"HealthcareService with id '{id}' not found.");

            await healthcareService.DeleteHealthcareAsync(id);

            return NoContent();
        }
    }
}
