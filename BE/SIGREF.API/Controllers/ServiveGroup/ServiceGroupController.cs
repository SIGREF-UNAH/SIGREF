using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Services.ServiceGroup;

namespace SIGREF.API.Controllers.ServiveGroup;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.cashier},{RolesConstants.auditor},{RolesConstants.ti}")]
public class ServiceGroupController(ServiceGroupService serviceGroupService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(PagedResultDto<ServiceGroupDto>))]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
     public async Task<ActionResult<ServiceGroupDto>> GetFiltered([FromQuery] ServiceGroupFilterDto filter)
    {
        var (items, pagination) = await serviceGroupService.GetFilteredServiceGroupsAsync(filter);

        var pagedDtos = new PagedResultDto<ServiceGroupDto>
        {
            Items = items,
            Pagination = pagination
        };

        return Ok(pagedDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier},{RolesConstants.admin},{RolesConstants.auditor},{RolesConstants.ti}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces<ServiceGroupDto>()]
    public async Task<IActionResult> GetById(string id)
    {
        var group = await serviceGroupService.GetServiceGroupByIdAsync(id);
        if (group == null) return NotFound($"ServiceGroup with id '{id}' not found.");

        return Ok(group);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces<ServiceGroupDto>()]
    [Authorize(Roles = $" {RolesConstants.admin}")]
    public async Task<IActionResult> Create([FromBody] CreateServiceGroupDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var list = createDto.ToFhirList();
        var createdList = await serviceGroupService.CreateServiceGroupAsync(list);

        // Usar GetServiceGroupByIdAsync para obtener el DTO completo con locations/services
        var createdDto = await serviceGroupService.GetServiceGroupByIdAsync(createdList.Id);

        return CreatedAtAction(nameof(GetById), new { id = createdList.Id }, createdDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [Produces<ServiceGroupDto>()]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateServiceGroupDto updateDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingList = await serviceGroupService.GetFhirListByIdAsync(id);
        if (existingList == null) return NotFound($"ServiceGroup with id '{id}' not found.");

        existingList.ApplyUpdate(updateDto);

        await serviceGroupService.UpdateServiceGroupAsync(existingList);

        var updatedGroup = await serviceGroupService.GetServiceGroupByIdAsync(id);

        return Ok(updatedGroup);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existingList = await serviceGroupService.GetFhirListByIdAsync(id);
        if (existingList == null) return NotFound($"ServiceGroup with id '{id}' not found.");

        await serviceGroupService.DeleteServiceGroupAsync(id);

        return NoContent();
    }
}