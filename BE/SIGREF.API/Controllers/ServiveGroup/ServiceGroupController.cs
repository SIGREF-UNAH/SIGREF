using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Services.ServiceGroup;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.ServiveGroup;

/// <summary>Gestiona paquetes de servicios médicos y su composición.</summary>
/// <remarks>Híbrido SIGREF + FHIR: usa la representación FHIR List para agrupar servicios y expone una vista de negocio propia.</remarks>
[Route("api/[controller]")]
[ApiController]
[Tags("Paquetes de Servicios Medicos - GestiÃ³n de Paquetes de Servicios Medicos")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class ServiceGroupController(HealthcareGroupService serviceGroupService) : ControllerBase
{
    [HttpGet]
    [EndpointName("GetServiceGroupList")]
    [EndpointSummary("Listar paquetes de servicios")]
    [EndpointDescription("Obtiene una lista paginada de paquetes de servicios médicos con sus filtros y relaciones.")]
    [Tags("HÍBRIDO - ServiceGroup / FHIR")]
    [ProducesResponseType(typeof(PagedResultDto<ServiceGroupDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    public async Task<ActionResult<ServiceGroupDto>> GetFiltered([FromQuery] ServiceGroupFilterDto filter)
    {
        var result = await serviceGroupService.GetFilteredAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EndpointName("GetServiceGroupById")]
    [EndpointSummary("Obtener un paquete por ID")]
    [EndpointDescription("Recupera un paquete de servicios y su composición a partir de su identificador FHIR.")]
    [Tags("HÍBRIDO - ServiceGroup / FHIR")]
    [ProducesResponseType(typeof(ServiceGroupDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    public async Task<IActionResult> GetById(string id)
    {
        var group = await serviceGroupService.GetByIdAsync(id);
        return Ok(group);
    }

    [HttpPost]
    [EndpointName("CreateServiceGroup")]
    [EndpointSummary("Crear un paquete de servicios")]
    [EndpointDescription("Crea un paquete basado en una representación FHIR List y devuelve su vista completa.")]
    [Tags("HÍBRIDO - ServiceGroup / FHIR")]
    [ProducesResponseType(typeof(ServiceGroupDto), StatusCodes.Status201Created)]
    [Authorize(Roles = $" {RolesConstants.admin}")]
    public async Task<IActionResult> Create([FromBody] CreateServiceGroupDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var list = createDto.ToFhirList();
        var createdList = await serviceGroupService.CreateAsync(list);

        // Usar GetServiceGroupByIdAsync para obtener el DTO completo con locations/services
        var createdDto = await serviceGroupService.GetByIdAsync(createdList.Id);

        return CreatedAtAction(nameof(GetById), new { id = createdList.Id }, createdDto);
    }

    [HttpPut("{id}")]
    [EndpointName("UpdateServiceGroupById")]
    [EndpointSummary("Actualizar un paquete de servicios")]
    [EndpointDescription("Actualiza la composición del paquete FHIR y devuelve la vista de negocio actualizada.")]
    [Tags("HÍBRIDO - ServiceGroup / FHIR")]
    [ProducesResponseType(typeof(ServiceGroupDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateServiceGroupDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingList = await serviceGroupService.GetFhirListByIdAsync(id);
        if (existingList == null) return NotFound($"ServiceGroup with id '{id}' not found.");

        existingList.ApplyUpdate(updateDto);

        await serviceGroupService.UpdateAsync(existingList);

        var updatedGroup = await serviceGroupService.GetByIdAsync(id);

        return Ok(updatedGroup);
    }

    [HttpDelete("{id}")]
    [EndpointName("DeleteServiceGroupById")]
    [EndpointSummary("Eliminar un paquete de servicios")]
    [EndpointDescription("Elimina el paquete de servicios identificado por su ID.")]
    [Tags("HÍBRIDO - ServiceGroup / FHIR")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existingList = await serviceGroupService.GetFhirListByIdAsync(id);
        if (existingList == null) return NotFound($"ServiceGroup with id '{id}' not found.");

        await serviceGroupService.DeleteAsync(id);

        return NoContent();
    }
}
