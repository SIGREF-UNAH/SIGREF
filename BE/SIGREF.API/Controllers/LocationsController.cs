using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Services;
using SIGREF.API.Extensions;
using SIGREF.API.Dtos;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Services.Location;

namespace SIGREF.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class LocationsController(LocationService locationService) : ControllerBase
{
    // GET: api/locations
    [HttpGet]
    [Authorize(Roles = Roles.admin)] // Permitido solo para ADMIN
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces<IEnumerable<LocationDto>>()]
    public async Task<IActionResult> Get()
    {
        var locations = await locationService.GetAllLocationsAsync();
        var locationDtos = locations.Select(location => location.ToDto());
        return Ok(locationDtos);
    }

    // GET api/locations/5
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.cashier)] // Permitido solo para CASHIER
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
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await locationService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound($"Location with id '{id}' not found.");

        await locationService.DeleteLocationAsync(id);
        return NoContent();
    }
}