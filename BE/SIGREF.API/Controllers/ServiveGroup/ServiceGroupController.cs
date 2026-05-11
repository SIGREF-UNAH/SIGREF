using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Services.ServiceGroup;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.ServiveGroup;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Paquetes de Servicios Medicos - Gestión de Paquetes de Servicios Medicos")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class ServiceGroupController(HealthcareGroupService serviceGroupService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetServiceGroupList",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ServiceGroup" }
    )]
    [ProducesResponseType(typeof(PagedResultDto<ServiceGroupDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    public async Task<ActionResult<ServiceGroupDto>> GetFiltered([FromQuery] ServiceGroupFilterDto filter)
    {
        var result = await serviceGroupService.GetFilteredAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        OperationId = "GetServiceGroupById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ServiceGroup" }
    )]
    [ProducesResponseType(typeof(ServiceGroupDto), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    public async Task<IActionResult> GetById(string id)
    {
        var group = await serviceGroupService.GetByIdAsync(id);
        return Ok(group);
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateServiceGroup",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ServiceGroup" }
    )]
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
    [SwaggerOperation(
        OperationId = "UpdateServiceGroupById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ServiceGroup" }
    )]
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
    [SwaggerOperation(
        OperationId = "DeleteServiceGroupById",
        Summary = "Obtener Ubicaciones",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "ServiceGroup" }
    )]
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