#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions.Common;
using SIGREF.API.Helpers;
using SIGREF.Common.Constants;

namespace SIGREF.API.Extensions;

public static class HealthcareExtensions
{

    // ============================================================
    //   FHIR  DTO
    // ============================================================
    public static HealthcareDto ToDto(this HealthcareService? healthcare, IFhirNamespaceService ns)
    {
        if (healthcare is null)
            return new HealthcareDto();

        string abbreviation = string.Empty;
        HealthcareScope scope = HealthcareScope.EXTERNAL; // default seguro

        if (healthcare.Extension != null)
        {
            // Abbreviation
            var abbreviationExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url == ns.HealthcareServiceAbbreviation);

            if (abbreviationExtension?.Value is Code abbr)
                abbreviation = abbr.Value ?? string.Empty;

            // Scope
            var scopeExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url == ns.HealthcareServiceScope);

            if (scopeExtension?.Value is Code scopeCode &&
                Enum.TryParse<HealthcareScope>(
                    scopeCode.Value,
                    ignoreCase: true,
                    out var parsedScope))
            {
                scope = parsedScope;
            }

        }

        return new HealthcareDto
        {
            Id = healthcare.Id ?? string.Empty,
            Identifier = healthcare.Identifier?.Select(i => i.ToDto()).ToList() ?? [],
            Active = healthcare.Active ?? true,
            Name = healthcare.Name ?? string.Empty,
            Comment = healthcare.Comment ?? string.Empty,
            Specialty = healthcare.Specialty?.Select(s => s.ToCodeableConceptDto()).ToList() ?? [],
            ProvidedBy = healthcare.ProvidedBy?.ToReferenceDto(),
            Location = healthcare.Location?.Select(l => l.ToReferenceDto()).ToList() ?? [],
            Abbreviation = abbreviation,
            Scope = scope,
            LastUpdated = healthcare.Meta?.LastUpdated?.UtcDateTime
        };
    }

    // ============================================================
    //   CREATE DTO to FHIR
    // ============================================================
    public static HealthcareService ToFhirHealthcare(this CreateHealthcareDto dto, IFhirNamespaceService ns)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var healthcare = new HealthcareService
        {
            Active = dto.Active,
            Identifier = dto.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [],
            Name = dto.Name,
            Comment = dto.Comment,
            Specialty = dto.Specialty?.Select(s => s.ToFhirCodeableConcept()).ToList() ?? [],
            ProvidedBy = dto.ProvidedBy?.ToFhirReference(),
            Location = dto.Location?.Select(l => l.ToFhirReference()).ToList() ?? [],
            Extension = new List<Extension>()
        };

        // Abbreviation (obligatoria)
        if (string.IsNullOrWhiteSpace(dto.Abbreviation))
            throw new ArgumentException("El servicio debe tener una abreviación");

        healthcare.Extension.Add(new Extension
        {
            Url = ns.HealthcareServiceAbbreviation,
            Value = new Code(dto.Abbreviation)
        });

        // Scope (internal | external)
        healthcare.Extension.Add(new Extension
        {
            Url = ns.HealthcareServiceScope,
            Value = new Code(dto.Scope.ToString().ToLowerInvariant())
        });


        return healthcare;
    }

    // ============================================================
    //   UPDATE DTO to FHIR
    // ============================================================
    public static HealthcareService ApplyUpdate(this HealthcareService existing, UpdateHealthcareDto update,  IFhirNamespaceService ns)
    {
        ArgumentNullException.ThrowIfNull(existing);

        if (update.WasSpecified(nameof(update.Identifier))) existing.Identifier = update.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Name))) existing.Name = update.Name;
        if (update.WasSpecified(nameof(update.Specialty))) existing.Specialty = update.Specialty?.Select(s => s.ToFhirCodeableConcept()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Location))) existing.Location = update.Location?.Select(l => l.ToFhirReference()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Active)) && update.Active.HasValue) existing.Active = update.Active.Value;
        if (update.WasSpecified(nameof(update.Comment))) existing.Comment = update.Comment;
        if (update.WasSpecified(nameof(update.ProvidedBy))) existing.ProvidedBy = update.ProvidedBy?.ToFhirReference();

        // Inicializar extensiones
        existing.Extension ??= new List<Extension>();

        // Abbreviation
        if (update.WasSpecified(nameof(update.Abbreviation)) && !string.IsNullOrEmpty(update.Abbreviation))
        {
            var abbreviationExtension = existing.Extension
                .FirstOrDefault(e => e.Url == ns.HealthcareServiceAbbreviation);

            if (abbreviationExtension != null)
                abbreviationExtension.Value = new Code(update.Abbreviation);
            else
                existing.Extension.Add(new Extension
                {
                    Url = ns.HealthcareServiceAbbreviation,
                    Value = new Code(update.Abbreviation)
                });
        }

        if (update.WasSpecified(nameof(update.Scope)) && update.Scope.HasValue)
        {
            var scopeValue = update.Scope.Value.ToString().ToLowerInvariant();
            var scopeExtension = existing.Extension.FirstOrDefault(e => e.Url == ns.HealthcareServiceScope);
            if (scopeExtension != null)
                scopeExtension.Value = new Code(scopeValue);
            else
                existing.Extension.Add(new Extension { Url = ns.HealthcareServiceScope, Value = new Code(scopeValue) });
        }
        

        return existing;
    }
}
