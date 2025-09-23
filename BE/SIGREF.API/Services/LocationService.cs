using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services;

public class LocationService(FhirClient fhirService)
{
    private const string ResourceType = nameof(Location);
    /// <summary>
    /// Obtiene un recurso <see cref="Location"/> por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso <see cref="Location"/> solicitado.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id"/> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el recurso no se encuentra o el servidor FHIR devuelve un error.</exception>
    /// <example>
    /// <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// Console.WriteLine(location.Name);
    /// </code>
    /// </example>
    public Task<Location> GetLocationByIdAsync(string id)
    {
        return fhirService.ReadAsync<Location>($"{ResourceType}/{id}");
    }
    /// <summary>
    /// Obtiene todos los recursos <see cref="Location"/> disponibles en el servidor FHIR.
    /// </summary>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es una colección de recursos <see cref="Location"/>.</returns>
    /// <remarks>
    /// Este método realiza una búsqueda sin filtros. En entornos con grandes volúmenes de datos, se recomienda paginar o filtrar.
    /// </remarks>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si ocurre un error durante la búsqueda en el servidor FHIR.</exception>
    /// <example>
    /// <code>
    /// var locations = await locationService.GetAllLocationsAsync();
    /// foreach (var loc in locations)
    /// {
    ///     Console.WriteLine(loc.Name);
    /// }
    /// </code>
    /// </example>
    public async Task<IEnumerable<Location>> GetAllLocationsAsync()
    {
        var searchResult = await fhirService.SearchAsync<Location>();
        return searchResult.Entry?.Select(e => e.Resource as Location).Where(l => l != null) ??
               Enumerable.Empty<Location>();
    }
    /// <summary>
    /// Crea un nuevo recurso <see cref="Location"/> en el servidor FHIR.
    /// Si el recurso no tiene un ID asignado, se genera uno automáticamente.
    /// También establece metadatos iniciales: fecha de última actualización y versión 1.
    /// </summary>
    /// <param name="location">El recurso <see cref="Location"/> a crear. No debe ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso creado, con metadatos actualizados.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location"/> es <c>null</c>.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el servidor FHIR rechaza la creación (por ejemplo, ID duplicado).</exception>
    /// <example>
    /// <code>
    /// var newLocation = new Location { Name = "Hospital Central" };
    /// var createdLocation = await locationService.CreateLocationAsync(newLocation);
    /// Console.WriteLine($"Creado con ID: {createdLocation.Id}");
    /// </code>
    /// </example>
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

        var result = await fhirService.CreateAsync(location);
        return result;
    }
    /// <summary>
    /// Actualiza un recurso <see cref="Location"/> existente en el servidor FHIR.
    /// Actualiza automáticamente la fecha de última modificación e incrementa el número de versión.
    /// Si el recurso no tiene metadatos, se inicializan.
    /// </summary>
    /// <param name="location">El recurso <see cref="Location"/> a actualizar. Debe tener un ID y no ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso actualizado con metadatos renovados.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="location"/> no tiene un ID asignado.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el recurso no existe o el servidor FHIR rechaza la actualización.</exception>
    /// <example>
    /// <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// location.Name = "Nuevo Nombre";
    /// var updatedLocation = await locationService.UpdateLocationAsync(location);
    /// Console.WriteLine($"Versión: {updatedLocation.Meta.VersionId}");
    /// </code>
    /// </example>
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

    /// <summary>
    /// Elimina un recurso <see cref="Location"/> del servidor FHIR por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location a eliminar. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id"/> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="Hl7.Fhir.Rest.FhirOperationException">Se lanza si el recurso no existe o no se puede eliminar.</exception>
    /// <example>
    /// <code>
    /// await locationService.DeleteLocationAsync("loc-123");
    /// </code>
    /// </example>
    public async Task DeleteLocationAsync(string id)
    {
         await fhirService.DeleteAsync($"{ResourceType}/{id}");
    }
}