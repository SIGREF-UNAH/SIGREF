using System.Runtime.Serialization;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Location;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Middleware;
using SIGREF.API.Services.Common;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirLocation = Hl7.Fhir.Model.Location;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Location;

public class LocationService : BaseFhirService, ILocationService
{
    private const string ResourceType = "Location";
    private readonly FhirClient _fhirClient;

    public LocationService(FhirService fhirService, IUserContextService userContext,
        IFhirNamespaceService ns)
        : base(userContext, ns)
    {
        _fhirClient = fhirService.GetFhirClient();
    }

    /// <summary>
    ///     Obtiene un recurso <see cref="FhirLocation" /> por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location. No debe ser nulo ni vacío.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el recurso <see cref="FhirLocation" />
    ///     solicitado.
    /// </returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id" /> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id" /> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no se encuentra o el servidor FHIR devuelve un error.</exception>
    /// <example>
    ///     <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// Console.WriteLine(location.Name);
    /// </code>
    /// </example>
    public async Task<LocationDto> GetLocationByIdAsync(string id)
    {
        try
        {
            // Intentar obtener el recurso desde el servidor médico
            var location = await _fhirClient.ReadAsync<FhirLocation>($"{ResourceType}/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "ResourceId", id },
                               { "ResourceType", "Location" }
                           });

            // Mapeo a DTO para asegurar que el frontend reciba data limpia y tipada
            return location.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "GET_LOCATION_BY_ID");
        }
    }

    /// <summary>
    ///     Crea un nuevo recurso <see cref="FhirLocation" /> en el servidor FHIR.
    ///     Si el recurso no tiene un ID asignado, se genera uno automáticamente.
    ///     También establece metadatos iniciales: fecha de última actualización y versión 1.
    /// </summary>
    /// <param name="location">El recurso <see cref="FhirLocation" /> a crear. No debe ser nulo.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el recurso creado, con metadatos
    ///     actualizados.
    /// </returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location" /> es <c>null</c>.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el servidor FHIR rechaza la creación (por ejemplo, ID duplicado).</exception>
    /// <example>
    ///     <code>
    /// var newLocation = new Location { Name = "Hospital Central" };
    /// var createdLocation = await locationService.CreateLocationAsync(newLocation);
    /// Console.WriteLine($"Creado con ID: {createdLocation.Id}");
    /// </code>
    /// </example>
    public async Task<LocationDto> CreateLocationAsync(CreateLocationDto dto)
    {
        try
        {
            var locationFhir = dto.ToFhirLocation();
            ApplyMeta(locationFhir, true);

            // Intento de Creación en el Servidor FHIR
            // IMPORTANTE: Capturamos la respuesta del servidor (created)
            var created = await _fhirClient.CreateAsync(locationFhir);

            // Retorno del DTO basado en la respuesta oficial del servidor
            // Esto garantiza que el DTO lleve el ID y el versionId generados
            return created.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "NEW_LOCATION", "CREATE_LOCATION");
        }
    }

    /// <summary>
    ///     Actualiza un recurso <see cref="FhirLocation" /> existente en el servidor FHIR.
    ///     Actualiza automáticamente la fecha de última modificación e incrementa el número de versión.
    ///     Si el recurso no tiene metadatos, se inicializan.
    /// </summary>
    /// <param name="location">El recurso <see cref="FhirLocation" /> a actualizar. Debe tener un ID y no ser nulo.</param>
    /// <returns>
    ///     Una tarea que representa la operación asíncrona. El resultado es el recurso actualizado con metadatos
    ///     renovados.
    /// </returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="location" /> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="location" /> no tiene un ID asignado.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no existe o el servidor FHIR rechaza la actualización.</exception>
    /// <example>
    ///     <code>
    /// var location = await locationService.GetLocationByIdAsync("loc-123");
    /// location.Name = "Nuevo Nombre";
    /// var updatedLocation = await locationService.UpdateLocationAsync(location);
    /// Console.WriteLine($"Versión: {updatedLocation.Meta.VersionId}");
    /// </code>
    /// </example>
    public async Task<LocationDto> UpdateLocationAsync(string id, UpdateLocationDto dto)
    {
        try
        {
            // Leer recurso existente (Fail Fast)
            var existing = await _fhirClient.ReadAsync<FhirLocation>($"{ResourceType}/{id}")
                           ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                           {
                               { "ResourceId", id },
                               { "ResourceType", "Location" }
                           });

            // Aplicar actualizaciones y metadatos
            existing.ApplyUpdate(dto);
            ApplyMeta(existing, false);

            // Enviar actualización al servidor médico
            var result = await _fhirClient.UpdateAsync(existing);

            // Retornar DTO fresco
            return result.ToDto();
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "UPDATE_LOCATION");
        }
    }

    /// <summary>
    ///     Elimina un recurso <see cref="FhirLocation" /> del servidor FHIR por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del recurso Location a eliminar. No debe ser nulo ni vacío.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="id" /> es <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Se lanza si <paramref name="id" /> es una cadena vacía o solo espacios en blanco.</exception>
    /// <exception cref="FhirOperationException">Se lanza si el recurso no existe o no se puede eliminar.</exception>
    /// <example>
    ///     <code>
    /// await locationService.DeleteLocationAsync("loc-123");
    /// </code>
    /// </example>
    public async Task DeleteLocationAsync(string id)
    {
        try
        {
            // Verificación previa (Fail Fast)
            _ = await _fhirClient.ReadAsync<FhirLocation>($"{ResourceType}/{id}")
                ?? throw new NotFoundException(MessageCodes.NotFound, new Dictionary<string, object>
                {
                    { "ResourceId", id },
                    { "ResourceType", "Location" }
                });

            // Ejecución del borrado en el servidor FHIR
            await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, id, "DELETE_LOCATION");
        }
    }

    // Filtrado
    public async Task<PagedResultDto<LocationDto>> GetFilteredLocationsAsync(LocationFilterDto filter)
    {
        try
        {
            // Validación de seguridad (Fail Fast)
            if (filter.PageSize > 500)
                throw new ValidationException(MessageCodes.ValidationError, new Dictionary<string, object>
                {
                    { "Field", "PageSize" },
                    { "MaxAllowed", 500 }
                });

            // Normalizar paginación usando el helper
            var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);
            var searchParams = new SearchParams();

            //Construcción de Filtros FHIR
            if (!string.IsNullOrWhiteSpace(filter.Name))
                searchParams.Add("name", filter.Name.Trim());

            if (filter.Status.HasValue)
                searchParams.Add("status", GetEnumMemberValue(filter.Status.Value));

            // Parámetros técnicos de paginación
            searchParams.Count = pageSize;
            searchParams.Add("_offset", offset.ToString());
            searchParams.Add("_total", "accurate");

            // Ejecución de búsqueda
            var bundle = await _fhirClient.SearchAsync<FhirLocation>(searchParams);

            // Convertir el Bundle de FHIR a nuestro PagedResult genérico
            var pagedResult = FhirPaginationHelper.ToPagedResult<FhirLocation>(bundle, pageNumber, pageSize);

            // Mapeo final de Items a DTO
            return new PagedResultDto<LocationDto>
            {
                Items = pagedResult.Items
                    .Select(l => l.ToDto())
                    .Where(dto => dto != null)!
                    .ToList(),
                Pagination = pagedResult.Pagination
            };
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, "SEARCH_FILTERED", "LOCATION_LIST");
        }
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