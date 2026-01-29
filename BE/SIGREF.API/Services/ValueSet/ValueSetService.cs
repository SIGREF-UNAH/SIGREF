using Hl7.Fhir.Rest;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Helpers;
using SIGREF.API.Services.Common;

namespace SIGREF.API.Services.ValueSet;

public class ValueSetService : IValueSetService
{
    private readonly FhirClient _client;

    public ValueSetService(FhirService fhirService)
    {
        _client = fhirService.GetFhirClient();
    }

    public async Task<ResponseDto<ValueSetDto>> GetCatalogAsync(CatalogType type)
    {
        var url = CatalogValueSetResolver.Resolve(type);

        var expanded = await _client.ExpandValueSetAsync(new Uri(url));

        var items = expanded.Expansion.Contains
            .Select(c => new ValueSetItemDto
            {
                Code = c.Code,
                Display = c.Display
            })
            .ToList();

        return new ResponseDto<ValueSetDto>
        {
            Status = true,
            StatusCode = 200,
            Data = new ValueSetDto
            {
                Url = url,
                Name = expanded.Name,
                Items = items
            },
            Message = "Catálogo cargado correctamente"
        };
    }
}