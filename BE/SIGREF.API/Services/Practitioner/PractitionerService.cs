using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions;
using SIGREF.API.Helpers;
using SIGREF.API.Services.PractitionerRole;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using Task = System.Threading.Tasks.Task;
using SIGREF.API.Dtos.PractitionerRole;

namespace SIGREF.API.Services.Practitioner;

public class PractitionerService : IPractitionerService
{
    private readonly FhirClient _fhirClient;
    private readonly IPractitionerRoleService _practitionerRoleService;
    private const string ResourceType = nameof(Practitioner); // Cambiar a nameof(Location) pero que no tenga conflicto con la clase o carpeta
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PatientService"/> con el cliente FHIR especificado.
    /// </summary>
    /// <param name="fhirClient">Cliente FHIR configurado para comunicarse con el servidor FHIR. No debe ser nulo.</param>
    /// <exception cref="System.ArgumentNullException">Se lanza si <paramref name="fhirClient"/> es <c>null</c>.</exception>
    public PractitionerService(FhirClient fhirClient, IPractitionerRoleService practitionerRoleService)
    {
        _fhirClient = fhirClient ?? throw new System.ArgumentNullException(nameof(fhirClient));
        _practitionerRoleService = practitionerRoleService ?? throw new System.ArgumentNullException(nameof(practitionerRoleService));
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

    public async Task<PractitionerDto> GetPractitionerByIdAsync(string id)
    {
        var practitioner = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}");
        var dto = practitioner.ToDto();

        // Obtener los roles del practitioner
        var roles = await _practitionerRoleService.GetByPractitionerIdAsync(id);
        dto.Roles = roles.ToList();

        return dto;
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

    public async Task<FhirPractitioner?> UpdatePractitionerWithDtoAsync(string id, UpdatePractitionerDto dto)
    {
        try
        {
            var existingPractitioner = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}");
            existingPractitioner.ApplyUpdate(dto);
            var result = await _fhirClient.UpdateAsync(existingPractitioner);
            return result;
        }
        catch (FhirOperationException)
        {
            return null;
        }
    }

    // Filtrar
    public async Task<PagedResult<PractitionerDto>> GetFilteredPractitionersAsync(PractitionerFilterDto filter)
    {
        // Normalizar paginación usando el helper
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(filter.PageNumber, filter.PageSize);

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

        // Obtener PagedResult del helper
        var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPractitioner>(bundle, pageNumber, pageSize);

        // Convertir Items a DTO y obtener roles para cada practitioner
        var items = new List<PractitionerDto>();
        foreach (var practitioner in pagedResult.Items)
        {
            var dto = practitioner.ToDto();
            if (dto != null && !string.IsNullOrEmpty(dto.Id))
            {
                // Obtener los roles del practitioner
                var roles = await _practitionerRoleService.GetByPractitionerIdAsync(dto.Id);
                dto.Roles = roles?.ToList() ?? new List<PractitionerRoleDto>();
                items.Add(dto);
            }
        }

        // Convertir Items a DTO
        var resultDto = new PagedResult<PractitionerDto>
        {
            Items = items,
            Pagination = pagedResult.Pagination
        };

        return resultDto;
    }
}

