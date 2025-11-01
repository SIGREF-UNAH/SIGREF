#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions.Common;
using System.Linq;

namespace SIGREF.API.Extensions;
public static class CommonExtensions
{
    // ────────────────────────────────────────────────
    // HUMAN NAME
    // ────────────────────────────────────────────────

    public static HumanNameDto ToDto(this HumanName name)
    {
        return new HumanNameDto
        {
            Use = name.Use,
            Text = name.Text,
            Family = name.Family,
            Given = name.Given?.ToList(),
            Prefix = name.Prefix?.ToList(),
            Suffix = name.Suffix?.ToList()
        };
    }

    public static HumanName ToFhirHumanName(this HumanNameDto dto)
    {
        return new HumanName
        {
            Use = dto.Use,
            Text = dto.Text,
            Family = dto.Family,
            Given = dto.Given?.ToArray(),
            Prefix = dto.Prefix?.ToArray(),
            Suffix = dto.Suffix?.ToArray()
        };
    }

    // ────────────────────────────────────────────────
    // IDENTIFIER
    // ────────────────────────────────────────────────

    public static IdentifierDto ToDto(this Identifier identifier)
    {
        return new IdentifierDto
        {
            Use = identifier.Use,
            Type = identifier.Type?.ToCodeableConceptDto(), 
            System = identifier.System,
            Value = identifier.Value
        };
    }

    public static Identifier ToFhirIdentifier(this IdentifierDto dto)
    {
        return new Identifier
        {
            Use = dto.Use,
            Type = dto.Type?.ToFhirCodeableConcept(), 
            System = dto.System,
            Value = dto.Value
        };
    }

    // ────────────────────────────────────────────────
    // REFERENCE
    // ────────────────────────────────────────────────

    public static ResourceReference ToFhirReference(this ReferenceDto dto)
    {
        var reference = new ResourceReference
        {
            Reference = dto.Reference,
            Display = dto.Display,
            Type = dto.Type
        };

        if (dto.Identifier != null)
        {
            reference.Identifier = dto.Identifier.ToFhirIdentifier();
        }

        return reference;
    }

    public static ReferenceDto ToReferenceDto(this ResourceReference reference)
        => new()
        {
            Reference = reference.Reference,
            Display = reference.Display,
            Type = reference.Type,
            Identifier = reference.Identifier?.ToDto()
        };

    // ────────────────────────────────────────────────
    // CODEABLE CONCEPT
    // ────────────────────────────────────────────────

    public static CodeableConcept ToFhirCodeableConcept(this CodeableConceptDto dto)
    {
        var coding = dto.Coding?.Select(c => new Coding(c.System, c.Code, c.Display)).ToList();

        return new CodeableConcept
        {
            Text = dto.Text,
            Coding = coding
        };
    }

    public static CodeableConceptDto ToCodeableConceptDto(this CodeableConcept cc)
    {
        return new CodeableConceptDto
        {
            Text = cc.Text,
            Coding = cc.Coding?.Select(c => new CodingDto
            {
                System = c.System,
                Code = c.Code,
                Display = c.Display
            }).ToList()
        };
    }

    // ────────────────────────────────────────────────
    // EXTENSION
    // ────────────────────────────────────────────────
    public static ExtensionDto ToDto(this Extension extension)
    {
        var dto = new ExtensionDto
        {
            Url = extension.Url
        };

        switch (extension.Value)
        {
            case CodeableConcept codeableConcept:
                dto.ValueCodeableConcept = codeableConcept.ToCodeableConceptDto();
                break;
            case FhirString fhirString:
                dto.ValueString = fhirString.Value;
                break;
            case FhirBoolean fhirBoolean:
                dto.ValueBoolean = fhirBoolean.Value;
                break;
            case Integer integer:
                dto.ValueInteger = integer.Value;
                break;
            case FhirDecimal fhirDecimal:
                dto.ValueDecimal = fhirDecimal.Value;
                break;
            case Date date:
                dto.ValueDate = date.ToDateTime();
                break;
        }

        return dto;
    }

    public static Extension ToFhirExtension(this ExtensionDto dto)
    {
        var extension = new Extension { Url = dto.Url };

        if (dto.ValueCodeableConcept != null)
        {
            extension.Value = dto.ValueCodeableConcept.ToFhirCodeableConcept();
        }
        else if (dto.ValueString != null)
        {
            extension.Value = new FhirString(dto.ValueString);
        }
        else if (dto.ValueBoolean.HasValue)
        {
            extension.Value = new FhirBoolean(dto.ValueBoolean.Value);
        }
        else if (dto.ValueInteger.HasValue)
        {
            extension.Value = new Integer(dto.ValueInteger.Value);
        }
        else if (dto.ValueDecimal.HasValue)
        {
            extension.Value = new FhirDecimal(dto.ValueDecimal.Value);
        }

        return extension;
    }



    /// <summary>
    /// Genera un Display genérico para cualquier DomainResource a partir de campos relevantes.
    /// </summary>
    /// <param name="resource">Recurso FHIR</param>
    /// <param name="fieldSelectors">Funciones que devuelven strings a combinar en el Display</param>
    public static void GenerateDisplay(this DomainResource resource, params Func<DomainResource, string?>[] fieldSelectors)
    {
        if (resource == null) return;
        if (fieldSelectors == null || fieldSelectors.Length == 0) return;

        // Extraer todos los valores no nulos y no vacíos
        var parts = fieldSelectors
            .Select(selector => selector(resource)?.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        // Si no hay valores, usar Id o tipo de recurso
        if (!parts.Any())
        {
            parts.Add(!string.IsNullOrEmpty(resource.Id) ? resource.Id : resource.TypeName);
        }

        // Combinar con separador
        var display = string.Join(" - ", parts);

        // Asignar a Text.Div
        resource.Text = new Narrative
        {
            Status = Narrative.NarrativeStatus.Generated,
            Div = $"<div>{display}</div>"
        };
    }
}
