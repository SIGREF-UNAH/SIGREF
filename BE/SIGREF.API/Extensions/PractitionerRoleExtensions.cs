using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.PractitionerRole;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class PractitionerRoleExtensions
{
    // DTO -> FHIR
    public static PractitionerRole ToFhirResource(this CreatePractitionerRoleDto dto)
    {
        var resource = new PractitionerRole
        {
            Active = dto.Active,
            Period = dto.Period != null ? new Period
            {
                StartElement = dto.Period.Start.HasValue ? new FhirDateTime(dto.Period.Start.Value) : null,
                EndElement = dto.Period.End.HasValue ? new FhirDateTime(dto.Period.End.Value) : null
            } : null,
            Practitioner = dto.Practitioner?.ToFhirReference(),
            Organization = dto.Organization?.ToFhirReference(),
            Location = dto.Location?.Select(x => x.ToFhirReference()).ToList(),
            Code = dto.Code?.Select(x => x.ToFhirCodeableConcept()).ToList(),
            Identifier = dto.Identifier?.Select(x => x.ToFhirIdentifier()).ToList(),

        };

        // Generar automáticamente el Display 
        resource.GenerateDisplay(
            r => ((PractitionerRole)r).Practitioner?.Display,          // nombre del practitioner
            r => ((PractitionerRole)r).Code?.FirstOrDefault()?.Text,   // rol
            r => ((PractitionerRole)r).Organization?.Display,          // organización
            r => ((PractitionerRole)r).Location?.FirstOrDefault()?.Display // ubicación
        );

        return resource;
    }

    // FHIR -> DTO
    public static PractitionerRoleDto ToDto(this PractitionerRole resource)
    {
        return new PractitionerRoleDto
        {
            Id = resource.Id,
            Active = resource.Active ?? true,
            Period = resource.Period != null ? new PeriodDto
            {
                Start = resource.Period.StartElement?.ToDateTimeOffset(TimeSpan.Zero).DateTime,
                End = resource.Period.EndElement?.ToDateTimeOffset(TimeSpan.Zero).DateTime
            } : null,
            Practitioner = resource.Practitioner?.ToReferenceDto(),
            Organization = resource.Organization?.ToReferenceDto(),
            Location = resource.Location?.Select(x => x.ToReferenceDto()).ToList(),
            Code = resource.Code?.Select(x => x.ToCodeableConceptDto()).ToList(),
            Identifier = resource.Identifier?.Select(x => x.ToDto()).ToList(),
            Display = resource.Text?.Div?.Replace("<div>", "").Replace("</div>", ""),
            LastUpdated = resource.Meta?.LastUpdated?.UtcDateTime,
            Endpoint = resource.Endpoint?.Select(x => x.ToReferenceDto()).ToList()
        };
    }
}
