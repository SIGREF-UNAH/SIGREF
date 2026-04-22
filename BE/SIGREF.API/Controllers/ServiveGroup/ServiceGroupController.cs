using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;
using SIGREF.API.Extensions;
using SIGREF.API.Services.ServiceGroup;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.ServiveGroup;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.cashier},{RolesConstants.auditor}")]
public class ServiceGroupController(HealthcareGroupService serviceGroupService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(PagedResultDto<ServiceGroupDto>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
     public async Task<ActionResult<ServiceGroupDto>> GetFiltered([FromQuery] ServiceGroupFilterDto filter)
    {
        var result = await serviceGroupService.GetFilteredAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor} ")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces<ServiceGroupDto>()]
    public async Task<IActionResult> GetById(string id)
    {
        var group = await serviceGroupService.GetByIdAsync(id);
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
        var createdList = await serviceGroupService.CreateAsync(list);

        // Usar GetServiceGroupByIdAsync para obtener el DTO completo con locations/services
        var createdDto = await serviceGroupService.GetByIdAsync(createdList.Id);

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

        await serviceGroupService.UpdateAsync(existingList);

        var updatedGroup = await serviceGroupService.GetByIdAsync(id);

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

        await serviceGroupService.DeleteAsync(id);

        return NoContent();
    }
}