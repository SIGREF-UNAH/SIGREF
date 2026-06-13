using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using Task = System.Threading.Tasks.Task;

namespace SIGREF.API.Services.FhirUtils;

public record IdentifierConflictDetail(string System, string Value, string ConflictingResourceId);

public interface IIdentifierValidationService
{
    /// <summary>
    /// Verifica en una sola consulta FHIR que ningún identificador de la lista
    /// ya exista en otro recurso del mismo tipo. Si hay conflictos, lanza una
    /// ConflictException con la lista completa de colisiones en ExtraData["Conflicts"].
    /// </summary>
    /// <typeparam name="TResource">Tipo de recurso FHIR (PractitionerRole, Patient, Practitioner, Organization…)</typeparam>
    /// <param name="identifiers">Identificadores a validar.</param>
    /// <param name="excludeId">ID del recurso en edición; se excluye del resultado para no auto-conflictar.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task ValidateUniquenessAsync<TResource>(
        IEnumerable<IdentifierDto> identifiers,
        string? excludeId = null,
        CancellationToken ct = default)
        where TResource : Resource, new();
}
