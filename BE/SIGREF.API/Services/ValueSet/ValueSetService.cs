using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Services.Common;
using SIGREF.Common.Dtos;
using SIGREF.Common.Helpers;
using SIGREF.Common.Types;

namespace SIGREF.API.Services.ValueSet;

public class ValueSetService : IValueSetService
{
    private readonly FhirClient _client;

    public ValueSetService(FhirService fhirService)
    {
        _client = fhirService.GetFhirClient();
    }

    public async Task<PagedResultDto<ValueSetItemDto>> GetCatalogAsync(CatalogType type, int page = 1,
        int pageSize = 100)
    {
        var canonicalUrl = CatalogValueSetResolver.Resolve(type);

        // FHIR usa offset (0-based), nosotros recibimos page (1-based)
        var offset = (page - 1) * pageSize;
        var expandUrl = $"ValueSet/$expand?url={canonicalUrl}&offset={offset}&count={pageSize}";
        var fullUri = new Uri(new Uri(_client.Endpoint.ToString()), expandUrl);
        Console.WriteLine($"[FHIR REQUEST]: {fullUri}");
        var result = await _client.GetAsync(expandUrl);

        // Mapeo de ítems desde expansion.contains
        var valueSet = result as Hl7.Fhir.Model.ValueSet
                       ?? throw new InvalidOperationException("FHIR server did not return a ValueSet.");

        // Mapeo de ítems desde expansion.contains
        var items = valueSet.Expansion?.Contains?
            .Select(c => new ValueSetItemDto
            {
                Code = c.Code,
                Display = c.Display
            }).ToList() ?? new List<ValueSetItemDto>();

        // Cálculos para el PaginationDto
        long totalItems = valueSet.Expansion?.Total ?? 0;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);


        return new PagedResultDto<ValueSetItemDto>
        {
            Items = items,
            Pagination = new PaginationDto
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            }
        };
    }
}