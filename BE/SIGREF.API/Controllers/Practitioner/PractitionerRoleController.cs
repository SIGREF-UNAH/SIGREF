using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.PractitionerC;

/// <summary>Gestiona los roles, especialidades y relaciones de los profesionales sanitarios.</summary>
/// <remarks>FHIR: representa y administra recursos PractitionerRole asociados a Practitioner y Organization.</remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Roles Empleados - Gestión de Roles Empleados")]
public class PractitionerRoleController : ControllerBase
{
    private readonly IPractitionerRoleService _prService;

    public PractitionerRoleController(IPractitionerRoleService prService)
    {
        _prService = prService;
    }

    // GET ALL
    [HttpGet]
    [EndpointName("GetPractitionerRoleList")]
    [EndpointSummary("Listar roles de profesionales")]
    [EndpointDescription("Obtiene una lista paginada de recursos PractitionerRole aplicando los filtros solicitados.")]
    [Tags("FHIR - PractitionerRole")]
    // Éxito: Especificamos el DTO de paginación con su tipo genérico
    [ProducesResponseType(typeof(PagedResultDto<PractitionerRoleDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PagedResultDto<PractitionerRoleDto>>> GetFiltered([FromQuery] PractitionerRoleFilterDto filters)
    {
        var pagedRoles = await _prService.GetFilteredAsync(filters);
        return Ok(pagedRoles);
    }

    // GET BY ID
    [HttpGet("{id}")]
    [EndpointName("GetPractitionerRoleById")]
    [EndpointSummary("Obtener un rol por ID")]
    [EndpointDescription("Recupera el recurso PractitionerRole identificado por su ID lógico en FHIR.")]
    [Tags("FHIR - PractitionerRole")]
    // Éxito: Retorna un solo objeto DTO
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> GetById(string id)
    {
        var role = await _prService.GetByIdAsync(id);
        return Ok(role);
    }

    // GET BY PRACTITIONER ID
    [HttpGet("practitioner/{id}")]
    [EndpointName("GetPractitionerRoleByPractitionerId")]
    [EndpointSummary("Listar roles de un profesional")]
    [EndpointDescription("Obtiene los roles FHIR asociados a un Practitioner específico.")]
    [Tags("FHIR - PractitionerRole")]
    // Especificamos que devuelve una colección (IEnumerable o List)
    [ProducesResponseType(typeof(IEnumerable<PractitionerRoleDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    public async Task<ActionResult<IEnumerable<PractitionerRoleDto>>> GetByPractitionerId(string id)
    {
        var roles = await _prService.GetByPractitionerIdAsync(id);
        return Ok(roles);
    }

    //CREATE
    [HttpPost]
    [EndpointName("CreatePractitionerRole")]
    [EndpointSummary("Crear un rol de profesional")]
    [EndpointDescription("Crea un recurso PractitionerRole con sus relaciones profesionales y organizacionales.")]
    [Tags("FHIR - PractitionerRole")]
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status201Created)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<ActionResult<PractitionerRoleDto>> Create([FromBody] CreatePractitionerRoleDto dto)
    {
        var result = await _prService.CreateAsync(dto);
        // Usamos CreatedAtAction para el estándar REST 201
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // UPDATE
    [HttpPut("{id}")]
    [EndpointName("UpdatePractitionerRoleById")]
    [EndpointSummary("Actualizar un rol de profesional")]
    [EndpointDescription("Actualiza el recurso PractitionerRole indicado por su ID lógico.")]
    [Tags("FHIR - PractitionerRole")]
    [ProducesResponseType(typeof(PractitionerRoleDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    [Produces<PractitionerRoleDto>()]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePractitionerRoleDto dto)
    {
        var result = await _prService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE
    [HttpDelete("{id}")]
    [EndpointName("DeletePractitionerRoleById")]
    [EndpointSummary("Eliminar un rol de profesional")]
    [EndpointDescription("Elimina el recurso PractitionerRole indicado por su ID lógico.")]
    [Tags("FHIR - PractitionerRole")]
    // Éxito: 204
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.admin}, {RolesConstants.ti}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _prService.DeleteAsync(id);
        return NoContent();
    }
}
