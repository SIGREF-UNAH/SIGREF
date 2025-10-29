#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Healthcare;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class HealthcareExtensions
{
    private const string DefaultAbbreviationExtensionUrl = "http://hl7.org/fhir/StructureDefinition/artifact-title";
    private const string DefaultCostExtensionUrl = "http://hl7.org/fhir/StructureDefinition/cqf-artifactComment";

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
                Comment = string.Empty,
                Specialty = [],
                ProvidedBy = null,
                Location = [],
                Extension = [],
                LastUpdated = null
            };

        // Extraer abbreviation y cost de las extensiones
        string abbreviation = string.Empty;
        decimal? cost = null;

        if (healthcare.Extension != null)
        {
            var abbreviationExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url.Equals(DefaultAbbreviationExtensionUrl, StringComparison.OrdinalIgnoreCase));
            if (abbreviationExtension?.Value is FhirString abbreviationValue)
            {
                abbreviation = abbreviationValue.Value ?? string.Empty;
            }

            var costExtension = healthcare.Extension
                .FirstOrDefault(e => e.Url.Equals(DefaultCostExtensionUrl, StringComparison.OrdinalIgnoreCase));
            if (costExtension?.Value is FhirDecimal costValue)
            {
                cost = costValue.Value;
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
            Extension = healthcare.Extension?.Select(e => e.ToDto()).ToList() ?? [],
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
            },
            Extension = new List<Hl7.Fhir.Model.Extension>()
        };

        // Validar y agregar extensión de abreviatura
        if (string.IsNullOrEmpty(dto.Abbreviation))
        {
            throw new ArgumentException("El servicio debe tener una extensión de abreviatura");
        }

        // Agregar extensión de abreviatura
        healthcare.Extension.Add(new Hl7.Fhir.Model.Extension
        {
            Url = DefaultAbbreviationExtensionUrl,
            Value = new FhirString(dto.Abbreviation)
        });

        // Agregar extensión de costo si se proporciona
        if (dto.Cost.HasValue)
        {
            healthcare.Extension.Add(new Hl7.Fhir.Model.Extension
            {
                Url = DefaultCostExtensionUrl,
                Value = new FhirDecimal(dto.Cost.Value)
            });
        }

        return healthcare;
    }

    // Aplicar UpdateHealthcareDto a un HealthcareService existente
    public static HealthcareService ApplyUpdate(this HealthcareService existing, UpdateHealthcareDto update)
    {
        ArgumentNullException.ThrowIfNull(existing);

        if (update.Identifier != null) existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();
        if (update.Active == true || update.Active == false) existing.Active = update.Active;
        if (!string.IsNullOrEmpty(update.Name)) existing.Name = update.Name;
        if (!string.IsNullOrEmpty(update.Comment)) existing.Comment = update.Comment;
        if (update.Specialty != null) existing.Specialty = update.Specialty.Select(s => s.ToFhirCodeableConcept()).ToList();
        if (update.ProvidedBy != null) existing.ProvidedBy = update.ProvidedBy.ToFhirReference();
        if (update.Location != null) existing.Location = update.Location.Select(l => l.ToFhirReference()).ToList();

        // Inicializar extensiones si no existen
        existing.Extension ??= new List<Hl7.Fhir.Model.Extension>();

        // Actualizar abreviatura
        if (!string.IsNullOrEmpty(update.Abbreviation))
        {
            var abbreviationExtension = existing.Extension
                .FirstOrDefault(e => e.Url.Equals(DefaultAbbreviationExtensionUrl, StringComparison.OrdinalIgnoreCase));

            if (abbreviationExtension != null)
            {
                abbreviationExtension.Value = new FhirString(update.Abbreviation);
            }
            else
            {
                existing.Extension.Add(new Hl7.Fhir.Model.Extension
                {
                    Url = DefaultAbbreviationExtensionUrl,
                    Value = new FhirString(update.Abbreviation)
                });
            }
        }

        // Actualizar costo
        if (update.Cost.HasValue)
        {
            var costExtension = existing.Extension
                .FirstOrDefault(e => e.Url.Equals(DefaultCostExtensionUrl, StringComparison.OrdinalIgnoreCase));

            if (costExtension != null)
            {
                costExtension.Value = new FhirDecimal(update.Cost.Value);
            }
            else
            {
                existing.Extension.Add(new Hl7.Fhir.Model.Extension
                {
                    Url = DefaultCostExtensionUrl,
                    Value = new FhirDecimal(update.Cost.Value)
                });
            }
        }
        else
        {
            // Remover extensión de costo si no se proporciona
            var costExtension = existing.Extension
                .FirstOrDefault(e => e.Url.Equals(DefaultCostExtensionUrl, StringComparison.OrdinalIgnoreCase));
            if (costExtension != null)
            {
                existing.Extension.Remove(costExtension);
            }
        }

        // Metadatos
        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTimeOffset.Now;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}