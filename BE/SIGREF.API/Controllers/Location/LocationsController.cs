using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Location;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Controllers.Location;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class LocationsController(ILocationService locationService) : ControllerBase
{
    // GET: api/locations
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    [Produces(typeof(PagedResultDto<LocationDto>))]
    public async Task<IActionResult> Get([FromQuery] LocationFilterDto filter)
    {
        var pagedLocations = await locationService.GetFilteredLocationsAsync(filter);
        var pagedLocationDtos = new PagedResultDto<LocationDto>
        {
            Items = pagedLocations.Items.Select(location => location.ToDto()),
            Pagination = pagedLocations.Pagination
        };
        return Ok(pagedLocationDtos);
    }

    // GET api/locations/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    [Produces<LocationDto>()]
    public async Task<IActionResult> GetById(int id)
    {
        var location = await locationService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound($"Location with id '{id}' not found.");

        var locationDto = location.ToDto();
        return Ok(locationDto);
    }

    [HttpPost]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces<LocationDto>()]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDto createLocationDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var location = createLocationDto.ToFhirLocation();
        var createdLocation = await locationService.CreateLocationAsync(location);
        var createdLocationDto = createdLocation.ToDto();
        return Ok(createdLocationDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces<LocationDto>()]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto updateLocationDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingLocation = await locationService.GetLocationByIdAsync(id);
        if (existingLocation == null)
            return NotFound($"Location with id '{id}' not found.");

        // Aplicar actualizaciones usando extension method
        existingLocation.ApplyUpdate(updateLocationDto);

        var updatedLocation = await locationService.UpdateLocationAsync(existingLocation);
        var updatedLocationDto = updatedLocation.ToDto();

        return Ok(updatedLocationDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{RolesConstants.admin}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await locationService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound($"Location with id '{id}' not found.");

        await locationService.DeleteLocationAsync(id);
        return NoContent();
    }
}