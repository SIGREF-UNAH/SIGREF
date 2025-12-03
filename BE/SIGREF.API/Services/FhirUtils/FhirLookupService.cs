using Hl7.Fhir.Model;
using FhirLocation = Hl7.Fhir.Model.Location;
using FhirOrganization = Hl7.Fhir.Model.Organization;
using FhirHealthcareService =  Hl7.Fhir.Model.HealthcareService;
using Hl7.Fhir.Rest;
using SIGREF.API.Services.Common;

namespace SIGREF.API.Services.FhirUtils;

public class FhirLookupService : IFhirLookupService
{
    private readonly FhirClient _client;

    public FhirLookupService(FhirService fhirService)
    {
        _client = fhirService.GetFhirClient();
    }

    private async Task<Dictionary<string, string>> BatchFetchAsync(
        string resourceType,
        IEnumerable<string> fhirIds)
    {
        // Usa HashSet para eliminar duplicados eficientemente sin materializar lista
        var ids = fhirIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();

        if (ids.Count == 0)
            return new Dictionary<string, string>(0);

        // Pre-allocate dictionary con capacidad exacta
        var result = new Dictionary<string, string>(ids.Count);

        // Procesa en lotes si hay muchos IDs (límite típico FHIR: ~200 IDs por query)
        const int batchSize = 200;

        foreach (var batch in ids.Chunk(batchSize))
        {
            var joined = string.Join(",", batch);

            var bundle = await _client.SearchAsync(
                resourceType,
                criteria: new[]
                {
                    $"_id={joined}",
                    "_summary=true",
                    "_elements=name" // Solo traer el campo name
                }
            );

            if (bundle.Entry == null)
                continue;

            // Procesa directamente sin almacenar referencias intermedias
            foreach (var entry in bundle.Entry)
            {
                if (entry.Resource is not DomainResource { Id: not null } r)
                    continue;

                var name = r switch
                {
                    FhirLocation { Name: not null } loc => loc.Name,
                    FhirHealthcareService { Name: not null } svc => svc.Name,
                    FhirOrganization { Name: not null } org => org.Name,
                    _ => r.Id // Fallback al ID si no hay nombre
                };

                result[r.Id] = name;
            }
        }

        return result;
    }

    public Task<Dictionary<string, string>> GetLocationNamesAsync(IEnumerable<string> fhirIds)
        => BatchFetchAsync("Location", fhirIds);

    public Task<Dictionary<string, string>> GetServiceNamesAsync(IEnumerable<string> fhirIds)
        => BatchFetchAsync("HealthcareService", fhirIds);

    public Task<Dictionary<string, string>> GetPackageNamesAsync(IEnumerable<string> fhirIds)
        => BatchFetchAsync("Organization", fhirIds);
}