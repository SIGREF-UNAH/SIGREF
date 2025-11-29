using Hl7.Fhir.Utility;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.ServiceGroup;

namespace SIGREF.API.Controllers.Shift;

public class ShiftController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(typeof(PagedResultDto<ShiftDto>))]
    public async Task<PagedResultDto<ServiceGroupDto>> GetFiltered([FromQuery] ShiftFilterDto filter)
    {
        //traer las turnos de nuestra db 
        //var shifts = _dbContext.Shifts.toListAsync();

        //var locationIds = data.Select(s => s.LocationId).Distinct().ToList();
        //TODO: mapear desde fhoir solo id y nombre
        // var locations = await fhirService.SearchAsync<Location>(new SearchParams().Add("_id", string.Join(",", locationIds)));
        // Normalizar paginación

        //var    shiftsWithLocations   =   shifts => lcoationId => locations.id
        //return shiftsWithLocations;

        var pagedDtos = new PagedResultDto<ServiceGroupDto>
        {
            Items = [],
            Pagination = new PaginationDto()
        };

        return pagedDtos;
    }
}

public record ShiftDto(
    Guid Id,
    Guid LocationId,
    string LocationName,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public class ShiftFilterDto : PagedFilterBase
{
    public string? Title { get; set; }
    public string? Status { get; set; }
}