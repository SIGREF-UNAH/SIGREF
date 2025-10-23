using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Practitioner;
public class PractitionerService : IPractitionerService
{
    private readonly FhirClient _fhirClient;
    private const string ResourceType = nameof(Practitioner); // Cambiar a nameof(Location) pero que no tenga conflicto con la clase o carpeta
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PatientService"/> con el cliente FHIR especificado.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para comunicarse con el servidor FHIR. No debe ser nulo.</param>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="fhirClient"/> es <c>null</c>.</exception>
    public PractitionerService(FhirClient fhirClient)
    {
        _fhirClient = fhirClient ?? throw new System.ArgumentNullException(nameof(fhirClient));
    }
    public async Task<FhirPractitioner> CreatePractitionerAsync(CreatePractitionerDto dto)
    {
        var practitioner = dto.ToFhirPractitioner();
        // Establecer metadatos
        //practitioner.Meta = new Meta
        //{
        //    LastUpdated = DateTimeOffset.Now,
        //    VersionId = "1"
        //};
        var created = await _fhirClient.CreateAsync(practitioner);
        return created;
    }

    public async Task DeletePractitionerAsync(string id)
    {
        await _fhirClient.DeleteAsync($"{ResourceType}/{id}");
    }

    public Task<FhirPractitioner> GetPractitionerByIdAsync(string id)
    {
        return _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}");
    }

    public async Task<FhirPractitioner> UpdatePractitionerAsync(string id, FhirPractitioner dto)
    {
        // Actualizar metadatos
        if (dto.Meta == null)
        {
            dto.Meta = new Meta();
        }

        dto.Meta.LastUpdated = DateTimeOffset.Now;

        // Incrementar versión si ya existe
        if (int.TryParse(dto.Meta.VersionId, out var currentVersion))
        {
            dto.Meta.VersionId = (currentVersion + 1).ToString();
        }
        else
        {
            dto.Meta.VersionId = "1";
        }

        var result = await _fhirClient.UpdateAsync(dto);
        return result;
    }

    // Filtrar
    public async Task<PagedResult<PractitionerDto>> GetFilteredPractitionersAsync(PractitionerFilterDto filter)
    {
        // Validar y normalizar parámetros de paginación
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
        var offset = (pageNumber - 1) * pageSize;

        var searchParams = new SearchParams();

        // Filtros básicos
        if (!string.IsNullOrWhiteSpace(filter.Name))
            searchParams.Add("name", filter.Name);

        if (filter.Active.HasValue)
            searchParams.Add("active", filter.Active.Value.ToString().ToLowerInvariant());

        if (filter.Gender.HasValue)
            searchParams.Add("gender", filter.Gender.Value.ToString().ToLowerInvariant());

        // Parámetros de paginación FHIR
        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        // Ejecutar búsqueda
        var bundle = await _fhirClient.SearchAsync<FhirPractitioner>(searchParams);

        // Mapear resultados a DTO
        var practitioners = bundle.Entry?
            .Select(e => (e.Resource as FhirPractitioner)?.ToDto())
            .Where(dto => dto != null)
            .ToList() ?? new List<PractitionerDto>();

        // Calcular totales
        var totalItems = bundle.Total ?? practitioners.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var pagination = new PaginationDto
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPrevious = pageNumber > 1,
            HasNext = bundle.NextLink != null || pageNumber < totalPages
        };

        // Retornar resultado paginado
        return new PagedResult<PractitionerDto>
        {
            Items = practitioners,
            Pagination = pagination
        };
    }
}

