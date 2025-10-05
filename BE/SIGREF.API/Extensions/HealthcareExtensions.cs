#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class HealthcareExtensions
{
    private const string AbbreviationExtensionUrl = "http://sigref.api/extensions/healthcare/abbreviation";
    private const string CostExtensionUrl = "http://sigref.api/extensions/healthcare/cost";

    // Convertir FHIR HealthcareService a HealthcareDto
    public static HealthcareDto ToDto(this HealthcareService? healthcare)
    {
        if (healthcare is null)
            return new HealthcareDto
            {
                Id = string.Empty,
                Identifier = [],
                Active = true,
                Name = string.Empty,
                Abbreviation = string.Empty,
                Comment = string.Empty,
                Cost = 0,
                Specialty = [],
                ProvidedBy = null,
                Location = [],
                LastUpdated = null
            };

        return new HealthcareDto
        {
            Id = healthcare.Id ?? string.Empty,
            Identifier = healthcare.Identifier?.Select(i => i.ToDto()).ToList() ?? [],
            Active = healthcare.Active ?? true,
            Name = healthcare.Name ?? string.Empty,
            Abbreviation = healthcare.GetStringExtension(AbbreviationExtensionUrl) ?? string.Empty,
            Comment = healthcare.Comment ?? string.Empty,
            Cost = healthcare.GetDecimalExtension(CostExtensionUrl) ?? 0,
            Specialty = healthcare.Specialty?.Select(s => s.ToDto()).ToList() ?? [],
            ProvidedBy = healthcare.ProvidedBy?.ToDto(),
            Location = healthcare.Location?.Select(l => l.ToDto()).ToList() ?? [],
            LastUpdated = healthcare.Meta?.LastUpdated?.DateTime
        };
    }

    // Convertir CreateHealthcareDto a FHIR HealthcareService
    public static HealthcareService ToFhirHealthcare(this CreateHealthcareDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var healthcare = new HealthcareService
        {
            Active = dto.Active,
            Identifier = dto.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [],
            Name = dto.Name ?? string.Empty,
            Comment = dto.Comment ?? string.Empty,
            Specialty = dto.Specialty?.Select(s => s.ToFhirCodeableConcept()).ToList() ?? [],
            ProvidedBy = dto.ProvidedBy?.ToFhirReference(),
            Location = dto.Location?.Select(l => l.ToFhirReference()).ToList() ?? [],
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            }
        };

        if (!string.IsNullOrEmpty(dto.Abbreviation))
            healthcare.AddOrUpdateExtension(AbbreviationExtensionUrl, new FhirString(dto.Abbreviation));

        healthcare.AddOrUpdateExtension(CostExtensionUrl, new FhirDecimal(dto.Cost));

        return healthcare;
    }

    // Aplicar UpdateHealthcareDto a un HealthcareService existente
    public static HealthcareService ApplyUpdate(this HealthcareService existing, UpdateHealthcareDto update)
    {
        ArgumentNullException.ThrowIfNull(existing);

        if (update.Identifier != null) existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();
        if (update.Active) existing.Active = update.Active;
        if (!string.IsNullOrEmpty(update.Name)) existing.Name = update.Name;
        if (!string.IsNullOrEmpty(update.Comment)) existing.Comment = update.Comment;
        if (update.Specialty != null) existing.Specialty = update.Specialty.Select(s => s.ToFhirCodeableConcept()).ToList();
        if (update.ProvidedBy != null) existing.ProvidedBy = update.ProvidedBy.ToFhirReference();
        if (update.Location != null) existing.Location = update.Location.Select(l => l.ToFhirReference()).ToList();

        // Extensiones personalizadas
        if (update.Abbreviation is not null)
            existing.AddOrUpdateExtension(AbbreviationExtensionUrl, new FhirString(update.Abbreviation));

        if (update.Cost.HasValue)
            existing.AddOrUpdateExtension(CostExtensionUrl, new FhirDecimal(update.Cost.Value));

        // Metadatos
        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTimeOffset.Now;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }

    // ──────────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────────

    // Obtener valor string de extensiones personalizadas
    private static string? GetStringExtension(this DomainResource resource, string url) =>
        (resource.Extension?.FirstOrDefault(e => e.Url == url)?.Value as FhirString)?.Value;

    // Obtener valor decimal de extensiones personalizadas
    private static decimal? GetDecimalExtension(this DomainResource resource, string url) =>
        (resource.Extension?.FirstOrDefault(e => e.Url == url)?.Value as FhirDecimal)?.Value;

    // Añadir o actualizar una extensión personalizada
    private static void AddOrUpdateExtension(this DomainResource resource, string url, DataType value)
    {
        resource.Extension ??= [];
        var existing = resource.Extension.FirstOrDefault(e => e.Url == url);
        if (existing is not null) existing.Value = value;
        else resource.Extension.Add(new Extension(url, value));
    }
}
