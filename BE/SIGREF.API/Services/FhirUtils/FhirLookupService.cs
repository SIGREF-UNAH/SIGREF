using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Caching.Memory;
using SIGREF.API.Services.Common;
using FhirHealthcareService = Hl7.Fhir.Model.HealthcareService;
using FhirLocation = Hl7.Fhir.Model.Location;
using FhirOrganization = Hl7.Fhir.Model.Organization;

namespace SIGREF.API.Services.FhirUtils;

public class FhirLookupService : IFhirLookupService
{
    private readonly FhirClient _client;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheTTL = TimeSpan.FromHours(12);

    public FhirLookupService(FhirService fhirService, IMemoryCache cache)
    {
        _client = fhirService.GetFhirClient();
        _cache = cache;
    }

    // ===============================
    // RESOLVER NOMBRE SIN USAR 'Alias'
    // ===============================
    private static string ResolveResourceName(DomainResource r)
    {
        return r switch
        {
            // 1. Location solo tiene Name
            FhirLocation loc when !string.IsNullOrWhiteSpace(loc.Name)
                => loc.Name,

            // 2. HealthcareService  Name  Type.Text  Category.Text  Id
            FhirHealthcareService svc when !string.IsNullOrWhiteSpace(svc.Name)
                => svc.Name,

            FhirHealthcareService svc when svc.Type?.Any() == true &&
                                           !string.IsNullOrWhiteSpace(svc.Type.First().Text)
                => svc.Type.First().Text,

            FhirHealthcareService svc when svc.Category?.Any() == true &&
                                           !string.IsNullOrWhiteSpace(svc.Category.First().Text)
                => svc.Category.First().Text,

            // 3. Organization  Name
            FhirOrganization org when !string.IsNullOrWhiteSpace(org.Name)
                => org.Name,

            // 4. Fallback final
            _ => r.Id ?? "N/A"
        };
    }

    private async Task<Dictionary<string, string>> BatchFetchAsync(
        string resourceType,
        IEnumerable<string> fhirIds)
    {
        var ids = fhirIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();

        if (ids.Count == 0)
            return new();

        var result = new Dictionary<string, string>(ids.Count);
        var missing = new List<string>();

        // ===========================
        // 1. Intentar desde cache
        // ===========================
        foreach (var id in ids)
        {
            var cacheKey = $"{resourceType}:{id}";

            if (_cache.TryGetValue(cacheKey, out string cachedName))
            {
                result[id] = cachedName;
            }
            else
            {
                missing.Add(id);
            }
        }

        // Si todo estaba en cache terminamos
        if (missing.Count == 0)
            return result;

        const int batchSize = 200;

        foreach (var batch in missing.Chunk(batchSize))
        {
            var joined = string.Join(",", batch);

            var bundle = await _client.SearchAsync(
                resourceType,
                criteria: new[]
                {
                    $"_id={joined}",
                    "_summary=true",
                    "_elements=name,type,category"
                }
            );

            if (bundle.Entry == null)
                continue;

            foreach (var entry in bundle.Entry)
            {
                if (entry.Resource is not DomainResource { Id: not null } r)
                    continue;

                string resolvedName = ResolveResourceName(r);

                result[r.Id] = resolvedName;

                // Guardar en cache
                _cache.Set($"{resourceType}:{r.Id}", resolvedName, CacheTTL);
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
