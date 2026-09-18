using SIGREF.API.Dtos.Location;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Location;

public interface ILocationService
{
    Task<PagedResultDto<LocationDto>> GetFilteredLocationsAsync(LocationFilterDto filter);
    Task<LocationDto> GetLocationByIdAsync(string id);
    Task<LocationDto> CreateLocationAsync(CreateLocationDto location);
    Task<LocationDto> UpdateLocationAsync(string id, UpdateLocationDto location);
    Task DeleteLocationAsync(string id);
}