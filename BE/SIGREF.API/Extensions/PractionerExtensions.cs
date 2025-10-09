using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;
public static class PractionerExtensions
{
    public static PractitionerDto ToDto(this Practitioner practitioner)
    {
        return new PractitionerDto
        {
            Id = practitioner.Id,
            Active = practitioner.Active ?? true,
            Name = practitioner.Name?.Select(n => n.ToDto()).ToList(),
            Gender = practitioner.Gender,
            BirthDate = practitioner.BirthDateElement?.ToDateTime(),
            Telecom = practitioner.Telecom?.Select(t => t.ToDto()).ToList(),
            Identifier = practitioner.Identifier?.Select(i => i.ToDto()).ToList(),
            LastUpdated = practitioner.Meta?.LastUpdated?.DateTime
        };
    }
    public static Practitioner ToFhirPractitioner(this CreatePractitionerDto dto)
    {
        return new Practitioner
        {
            Active = dto.Active,
            Name = dto.Name?.Select(n => n.ToFhirHumanName()).ToList() ?? [],
            Gender = dto.Gender,
            BirthDateElement = dto.BirthDate.ToFhirDate(),
            Telecom = dto.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? [],
            Identifier = dto.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [],
            Meta = new Meta
            {
                LastUpdated = DateTimeOffset.Now,
                VersionId = "1"
            }
        };
    }

    public static Practitioner ApplyUpdate(this Practitioner existing, UpdatePractitionerDto update)
    {
        if (update.Active.HasValue)
            existing.Active = update.Active.Value;

        if (update.Name != null)
            existing.Name = update.Name.Select(n => n.ToFhirHumanName()).ToList();

        if (!string.IsNullOrEmpty(update.Gender?.ToString()))
            existing.Gender = update.Gender;

        if (update.BirthDate.HasValue)
            existing.BirthDateElement = update.BirthDate.ToFhirDate();

        if (update.Telecom != null)
            existing.Telecom = update.Telecom.Select(t => t.ToFhirContactPoint()).ToList();

        if (update.Identifier != null)
            existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();
        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTimeOffset.Now;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}

