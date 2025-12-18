#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Constants;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class HealthcareExtensions
{
    private const string AbbreviationExtensionUrl =
        FhirNamespaces.HealthcareServiceAbbreviation;

    private const string ScopeExtensionUrl =
        FhirNamespaces.HealthcareServiceScope;


    // ============================================================
    //   FHIR  DTO
    // ============================================================
    public static HealthcareDto ToDto(this HealthcareService? healthcare)
    {
        if (healthcare is null)
            return new HealthcareDto();

        string abbreviation = string.Empty;
        HealthcareScope scope = HealthcareScope.EXTERNAL; // default seguro

        if (healthcare.Extension != null)
        {
            // Abbreviation
            var abbreviationExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url == AbbreviationExtensionUrl);

            if (abbreviationExtension?.Value is Code abbr)
                abbreviation = abbr.Value ?? string.Empty;

            // Scope
            var scopeExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url == ScopeExtensionUrl);

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
            LastUpdated = healthcare.Meta?.LastUpdated?.DateTime
        };
    }

    // ============================================================
    //   CREATE DTO to FHIR
    // ============================================================
    public static HealthcareService ToFhirHealthcare(this CreateHealthcareDto dto)
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
            Url = AbbreviationExtensionUrl,
            Value = new Code(dto.Abbreviation)
        });

        // Scope (internal | external)
        healthcare.Extension.Add(new Extension
        {
            Url = ScopeExtensionUrl,
            Value = new Code(dto.Scope.ToString().ToLowerInvariant())
        });


        return healthcare;
    }

    // ============================================================
    //   UPDATE DTO to FHIR
    // ============================================================
    public static HealthcareService ApplyUpdate(this HealthcareService existing, UpdateHealthcareDto update)
    {
        ArgumentNullException.ThrowIfNull(existing);

        if (update.Identifier != null) existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();
        if (!string.IsNullOrEmpty(update.Name)) existing.Name = update.Name;
        if (update.Specialty != null) existing.Specialty = update.Specialty.Select(s => s.ToFhirCodeableConcept()).ToList();
        if (update.Location != null) existing.Location = update.Location.Select(l => l.ToFhirReference()).ToList();

        existing.Active = update.Active;
        existing.Comment = update.Comment;
        existing.ProvidedBy = update.ProvidedBy?.ToFhirReference();

        // Inicializar extensiones
        existing.Extension ??= new List<Extension>();

        // Abbreviation
        if (!string.IsNullOrEmpty(update.Abbreviation))
        {
            var abbreviationExtension = existing.Extension
                .FirstOrDefault(e => e.Url == AbbreviationExtensionUrl);

            if (abbreviationExtension != null)
                abbreviationExtension.Value = new Code(update.Abbreviation);
            else
                existing.Extension.Add(new Extension
                {
                    Url = AbbreviationExtensionUrl,
                    Value = new Code(update.Abbreviation)
                });
        }

        // Scope (siempre se guarda)
        var scopeValue = update.Scope.ToString().ToLowerInvariant();

        var scopeExtension = existing.Extension.FirstOrDefault(e => e.Url == ScopeExtensionUrl);

        if (scopeExtension != null)
        {
            scopeExtension.Value = new Code(scopeValue);

        }
        else
        {
            existing.Extension.Add(new Extension
            {
                Url = ScopeExtensionUrl,
                Value = new Code(scopeValue)
            });
        }

        // Metadatos
        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTimeOffset.Now;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}