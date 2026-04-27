using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Dtos.Location;
using SIGREF.Common.Dtos;
using FhirLocation = Hl7.Fhir.Model.Location;

namespace SIGREF.API.Services.Location
{
    public interface ILocationService
    {
        Task<PagedResultDto<FhirLocation>> GetFilteredLocationsAsync(LocationFilterDto filter);
        Task<FhirLocation> GetLocationByIdAsync(int id);
        Task<FhirLocation> CreateLocationAsync(FhirLocation location);
        Task<FhirLocation> UpdateLocationAsync(FhirLocation location);
        Task DeleteLocationAsync(int id);
    }
}
