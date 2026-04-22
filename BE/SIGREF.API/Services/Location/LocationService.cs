using System.Runtime.Serialization;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Common;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirLocation = Hl7.Fhir.Model.Location;
using Task = System.Threading.Tasks.Task;
namespace SIGREF.API.Services.Location;

public class LocationService : BaseFhirService
{
    private readonly FhirClient _fhirClient;

    public LocationService(FhirService fhirService,  IUserContextService userContext,       
        IFhirNamespaceService ns)             
        : base(userContext, ns)    
    {
        _fhirClient = fhirService.GetFhirClient();
    }

    private const string ResourceType = "Location";
    /// <summary>
    /// Obtiene un recurso <see cref="FhirLocation"/> por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso <see cref="FhirLocation"/> solicitado.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id"/> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no se encuentra o el servidor FHIR devuelve un error.</exception>
    /// <example>
    /// <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// Console.WriteLine(location.Name);
    /// </code>
    /// </example>
    public Task<FhirLocation> GetLocationByIdAsync(int id)

    {
        return _fhirClient.ReadAsync<FhirLocation>($"{ResourceType}/{id}");
    }
    /// <summary>
    /// Crea un nuevo recurso <see cref="FhirLocation"/> en el servidor FHIR.
    /// Si el recurso no tiene un ID asignado, se genera uno automáticamente.
    /// También establece metadatos iniciales: fecha de última actualización y versión 1.
    /// </summary>
    /// <param name="location">El recurso <see cref="FhirLocation"/> a crear. No debe ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso creado, con metadatos actualizados.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location"/> es <c>null</c>.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el servidor FHIR rechaza la creación (por ejemplo, ID duplicado).</exception>
    /// <example>
    /// <code>
    /// var newLocation = new Location { Name = "Hospital Central" };
    /// var createdLocation = await locationService.CreateLocationAsync(newLocation);
    /// Console.WriteLine($"Creado con ID: {createdLocation.Id}");
    /// </code>
    /// </example>
    public async Task<FhirLocation> CreateLocationAsync(FhirLocation location)
    {
        // Generar un ID si no tiene uno
        if (string.IsNullOrEmpty(location.Id))
        {
            location.Id = Guid.NewGuid().ToString();
        }
        ApplyMeta(location, isCreate:true);

        await _fhirClient.CreateAsync(location);
        return location;
    }
    /// <summary>
    /// Actualiza un recurso <see cref="FhirLocation"/> existente en el servidor FHIR.
    /// Actualiza automáticamente la fecha de última modificación e incrementa el número de versión.
    /// Si el recurso no tiene metadatos, se inicializan.
    /// </summary>
    /// <param name="location">El recurso <see cref="FhirLocation"/> a actualizar. Debe tener un ID y no ser nulo.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el recurso actualizado con metadatos renovados.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="location"/> no tiene un ID asignado.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no existe o el servidor FHIR rechaza la actualización.</exception>
    /// <example>
    /// <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// location.Name = "Nuevo Nombre";
    /// var updatedLocation = await locationService.UpdateLocationAsync(location);
    /// Console.WriteLine($"Versión: {updatedLocation.Meta.VersionId}");
    /// </code>
    /// </example>
    public async Task<FhirLocation> UpdateLocationAsync(FhirLocation location)
    {
        ApplyMeta(location,isCreate:false);
        var result = await _fhirClient.UpdateAsync(location);
        return result;
    }

    /// <summary>
    /// Elimina un recurso <see cref="FhirLocation"/> del servidor FHIR por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location a eliminar. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id"/> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id"/> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no existe o no se puede eliminar.</exception>
    /// <example>
    /// <code>
    /// await locationService.DeleteLocationAsync("loc-123");
    /// </code>
    /// </example>
    public async Task DeleteLocationAsync(int id)
    {
        await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
    }

    // Filtrado
    public async Task<PagedResultDto<FhirLocation>> GetFilteredLocationsAsync(LocationFilterDto filter)
    {
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

        var searchParams = new SearchParams();

        if (!string.IsNullOrWhiteSpace(filter.Name))
            searchParams.Add("name", filter.Name);

        if (filter.Status.HasValue)
            searchParams.Add("status", GetEnumMemberValue(filter.Status.Value));

        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        var bundle = await _fhirClient.SearchAsync<FhirLocation>(searchParams);

        return FhirPaginationHelper.ToPagedResult<FhirLocation>(bundle, pageNumber, pageSize);
    }

    // Auxiliar para obtener el valor de [EnumMember]
    private static string GetEnumMemberValue(Enum enumValue)
    {
        var type = enumValue.GetType();
        var info = type.GetField(enumValue.ToString());
        var attr = info?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                        .Cast<EnumMemberAttribute>()
                        .FirstOrDefault();

        return attr?.Value ?? enumValue.ToString().ToLowerInvariant();
    }
}