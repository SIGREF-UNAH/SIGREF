using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Extensions;
using SIGREF.API.Fhir;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.Common.Dtos;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using FhirPractitioner = Hl7.Fhir.Model.Practitioner;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.Practitioner;

public class PractitionerService : BaseFhirService, IPractitionerService
{
    private readonly FhirClient _fhirClient;
    private readonly IPractitionerRoleService _practitionerRoleService;
    private const string ResourceType = nameof(Practitioner); 

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PractitionerService"/> con los clientes y servicios necesarios.
    /// </summary>
    public PractitionerService(
        FhirClient fhirClient, 
        IPractitionerRoleService practitionerRoleService,
        IUserContextService userContext,       
        IFhirNamespaceService ns)             
        : base(userContext, ns)               
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

    public async Task<FhirPractitioner?> UpdatePractitionerAsync(string id, UpdatePractitionerDto dto)
    {
        try
        {
            var existingPractitioner = await _fhirClient.ReadAsync<FhirPractitioner>($"{ResourceType}/{id}");
            existingPractitioner.ApplyUpdate(dto);
            ApplyMeta(existingPractitioner,isCreate:false);
            var result = await _fhirClient.UpdateAsync(existingPractitioner);
            return result;
        }
        catch (FhirOperationException)
        {
            return null;
        }
    }

    // Filtrar
    public async Task<PagedResultDto<PractitionerDto>> GetFilteredPractitionersAsync(PractitionerFilterDto filter)
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
        var resultDto = new PagedResultDto<PractitionerDto>
        {
            Items = items,
            Pagination = pagedResult.Pagination
        };

        return resultDto;
    }
}

