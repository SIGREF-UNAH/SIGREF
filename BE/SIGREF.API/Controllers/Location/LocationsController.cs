using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Extensions;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Constants;
using SIGREF.API.Services.Location;

namespace SIGREF.API.Controllers.Location;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class LocationsController(LocationService locationService) : ControllerBase
{
    // GET: api/locations
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(IEnumerable<LocationDto>))]
    public async Task<IActionResult> Get([FromQuery] LocationFilterDto filter)
    {
        var locations = await locationService.GetFilteredLocationsAsync(filter);
        var locationDtos = locations.Select(location => location.ToDto());
        return Ok(locationDtos);
    }

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active", "suspended", "inactive"
    };

    // GET api/locations/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces<LocationDto>()]
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
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await locationService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound($"Location with id '{id}' not found.");

        await locationService.DeleteLocationAsync(id);
        return NoContent();
    }
}