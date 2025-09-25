using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services;

public class LocationService(FhirClient fhirService)
{
    private const string ResourceType = nameof(Location);

    public Task<Location> GetLocationByIdAsync(Guid id)
    {
        return fhirService.ReadAsync<Location>($"{ResourceType}/{id}");
    }

    public async Task<IEnumerable<Location>> GetAllLocationsAsync()
    {
        var searchResult = await fhirService.SearchAsync<Location>();
        return searchResult.Entry?.Select(e => e.Resource as Location).Where(l => l != null) ??
               Enumerable.Empty<Location>();
    }

    public async Task<Location> CreateLocationAsync(Location location)
    {
        // Generar un ID si no tiene uno
        if (string.IsNullOrEmpty(location.Id))
        {
            location.Id = Guid.NewGuid().ToString();
        }

        // Establecer metadatos
        location.Meta = new Meta
        {
            LastUpdated = DateTimeOffset.Now,
            VersionId = "1"
        };

         await fhirService.CreateAsync(location);
        return location;
    }

    public async Task<Location> UpdateLocationAsync(Location location)
    {
        // Actualizar metadatos
        if (location.Meta == null)
        {
            location.Meta = new Meta();
        }

        location.Meta.LastUpdated = DateTimeOffset.Now;

        // Incrementar versión si ya existe
        if (int.TryParse(location.Meta.VersionId, out var currentVersion))
        {
            location.Meta.VersionId = (currentVersion + 1).ToString();
        }
        else
        {
            location.Meta.VersionId = "1";
        }

        var result = await fhirService.UpdateAsync(location);
        return result;
    }

    public async Task DeleteLocationAsync(Guid id)
    {
         await fhirService.DeleteAsync($"{ResourceType}/{id}");
    }
}