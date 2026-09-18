using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Services.Practitioner;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.PractitionerC;

/// <summary>Gestiona los profesionales sanitarios registrados en la plataforma.</summary>
/// <remarks>FHIR: representa y administra recursos Practitioner, usados también para vincular usuarios de Keycloak.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Practitioners")]
public class PractitionerController : ControllerBase
{
    private readonly IPractitionerService _practitionerService;

    public PractitionerController(IPractitionerService practitionerService)
    {
        _practitionerService = practitionerService;
    }

    // GET: api/practitioner
    [HttpGet]
    [EndpointName("GetPractitionerList")]
    [EndpointSummary("Listar profesionales")]
    [EndpointDescription("Obtiene una lista paginada de recursos Practitioner aplicando los filtros solicitados.")]
    [Tags("Practitioners", "FHIR")]
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
    [EndpointName("GetPractitionerById")]
    [EndpointSummary("Obtener un profesional por ID")]
    [EndpointDescription("Recupera el recurso Practitioner identificado por su ID lógico en FHIR.")]
    [Tags("Practitioners", "FHIR")]
    [ProducesResponseType( typeof(PractitionerDto) , StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.auditor} , {RolesConstants.ti}")]
    public async Task<IActionResult> GetById(string id)
    {
        var prectitioner = await _practitionerService.GetPractitionerByIdAsync(id);
        return Ok(prectitioner);
    }

    // POST: api/practitioner
    [HttpPost]
    [EndpointName("CreatePractitioner")]
    [EndpointSummary("Crear un profesional")]
    [EndpointDescription("Crea un nuevo recurso Practitioner con los datos profesionales proporcionados.")]
    [Tags("Practitioners", "FHIR")]
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
    [EndpointName("UpdatePractitionerById")]
    [EndpointSummary("Actualizar un profesional")]
    [EndpointDescription("Actualiza el recurso Practitioner indicado por su ID lógico.")]
    [Tags("Practitioners", "FHIR")]
    [ProducesResponseType(typeof(PractitionerDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin} , {RolesConstants.ti}")]
    public async Task<IActionResult> UpdatePractitioner(string id, [FromBody] UpdatePractitionerDto updatePractitionerDto)
    {
        var updatedPractitioner = await _practitionerService.UpdatePractitionerAsync(id, updatePractitionerDto);
        return Ok(updatedPractitioner);
    }

    // DELETE: api/practitioner/{id}
    [HttpDelete("{id}")]
    [EndpointName("DeletePractitionerById")]
    [EndpointSummary("Eliminar un profesional")]
    [EndpointDescription("Elimina el recurso Practitioner indicado por su ID lógico.")]
    [Tags("Practitioners", "FHIR")]
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
