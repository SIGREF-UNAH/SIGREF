using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Services.Practitioner;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.PractitionerC;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Empleados - Gestión de Empleados")]
public class PractitionerController : ControllerBase
{
    private readonly IPractitionerService _practitionerService;

    public PractitionerController(IPractitionerService practitionerService)
    {
        _practitionerService = practitionerService;
    }

    // GET: api/practitioner
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetPractitionerList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Practitioner" }
    )]
    [ProducesResponseType(typeof(PagedResultDto<PractitionerDto>) , StatusCodes.Status200OK)]
    [Authorize(Roles = $" {RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
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
    [SwaggerOperation(
        OperationId = "GetPractitionerById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Practitioner" }
    )]
    [ProducesResponseType( typeof(PractitionerDto) , StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<IActionResult> GetById(string id)
    {
        var prectitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        return Ok(prectitioner);
    }

    // POST: api/practitioner
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreatePractitioner",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Practitioner" }
    )]
    [ProducesResponseType( typeof(PractitionerDto) , StatusCodes.Status201Created)]
    [Authorize(Roles = $" {RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<IActionResult> CreatePractitioner([FromBody] CreatePractitionerDto createPractitionerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdPractitioner = await _practitionerService.CreatePractitionerAsync(createPractitionerDto);
        return CreatedAtAction(nameof(GetById), new { id = createdPractitioner.Id }, createdPractitioner);
    }

    // PUT: api/practitioner/{id}
    [HttpPut("{id}")]
    [SwaggerOperation(
        OperationId = "UpdatePractitionerById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Practitioner" }
    )]
    [ProducesResponseType(typeof(PractitionerDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<IActionResult> UpdatePractitioner(string id, [FromBody] UpdatePractitionerDto updatePractitionerDto)
    {
        var updatedPractitioner = await _practitionerService.UpdatePractitionerAsync(id, updatePractitionerDto);
        return Ok(updatedPractitioner);
    }

    // DELETE: api/practitioner/{id}
    [HttpDelete("{id}")]
    [SwaggerOperation(
        OperationId = "DeletePractitionerById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Practitioner" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<IActionResult> DeletePractitioner(string id)
    {
        // 1. Verificar que el paciente exista
        var existingPractitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        if (existingPractitioner.Id == null)
            return NotFound($"Patient with id '{id}' not found.");

        // 2. Eliminar
        await _practitionerService.DeletePractitionerAsync(id);

        // 3. Devolver 204 No Content
        return NoContent();
    }
}

