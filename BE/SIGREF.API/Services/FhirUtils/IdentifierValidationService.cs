using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Middleware;
using SIGREF.Common.Exceptions;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.FhirUtils;

public class IdentifierValidationService(FhirClient _fhirClient) : IIdentifierValidationService
{
    public async Task ValidateUniquenessAsync<TResource>(
        IEnumerable<IdentifierDto> identifiers,
        string? excludeId = null,
        CancellationToken ct = default)
        where TResource : Resource, new()
    {
        // Filtrar vacíos y normalizar conservando el valor original para el reporte
        var candidates = identifiers
            .Where(i => !string.IsNullOrWhiteSpace(i.System) && !string.IsNullOrWhiteSpace(i.Value))
            .Select(i => new
            {
                Original = (System: i.System!.Trim(), Value: i.Value!.Trim()),
                Normalized = (System: i.System!.Trim().ToLowerInvariant(), Value: i.Value!.Trim().ToLowerInvariant())
            })
            .ToList();

        if (candidates.Count == 0) return;

        // Una sola consulta FHIR: valores separados por coma = OR en el estándar FHIR R4
        // Se usan los valores originales (trimmed) porque HAPI FHIR indexa tokens con su capitalización original.
        // La normalización a minúsculas se aplica solo en HasIdentifier para el matching local.
        var identifierParam = string.Join(",", candidates.Select(c => $"{c.Original.System}|{c.Original.Value}"));
        var searchParams = new SearchParams().Add("identifier", identifierParam);

        Bundle? bundle;
        try
        {
            bundle = await _fhirClient.SearchAsync<TResource>(searchParams, ct);
        }
        catch (FhirOperationException ex)
        {
            throw FhirExceptionMapper.Map(ex, identifierParam, "VALIDATE_UNIQUE_IDENTIFIERS");
        }

        if (bundle?.Entry is not { Count: > 0 }) return;

        // En UPDATE excluimos el propio recurso para que no se auto-reporte como conflicto
        var conflictingResources = bundle.Entry
            .Select(e => e.Resource)
            .Where(r => r is not null && r.Id != excludeId)
            .ToList();

        if (conflictingResources.Count == 0) return;

        // Recopilar TODOS los conflictos antes de lanzar la excepción
        var conflicts = candidates
            .Select(c =>
            {
                var hit = conflictingResources
                    .FirstOrDefault(r => HasIdentifier(r!, c.Normalized.System, c.Normalized.Value));

                return hit is null
                    ? null
                    : new IdentifierConflictDetail(
                        c.Original.System,
                        c.Original.Value,
                        $"{hit.TypeName}/{hit.Id}");
            })
            .Where(c => c is not null)
            .ToList();

        if (conflicts.Count == 0) return;

        throw new ConflictException(MessageCodes.DbUniqueConstraint, new Dictionary<string, object>
        {
            { "Conflicts", conflicts }
        });
    }

    // Acceso genérico a la propiedad Identifier sin depender de un tipo FHIR específico.
    // Todos los recursos con identificadores (PractitionerRole, Patient, Practitioner,
    // Organization) exponen List<Identifier> bajo el mismo nombre de propiedad.
    private static bool HasIdentifier(Resource resource, string system, string value)
    {
        var identifiers = resource.GetType()
            .GetProperty("Identifier")
            ?.GetValue(resource) as List<Identifier>;

        return identifiers?.Any(id =>
            string.Equals(id.System?.Trim(), system, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(id.Value?.Trim(), value, StringComparison.OrdinalIgnoreCase)) == true;
    }
}