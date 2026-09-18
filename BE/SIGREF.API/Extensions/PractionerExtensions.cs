using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Dtos.Practitioner;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;
public static class PractionerExtensions
{
    public static PractitionerDto ToDto(this Practitioner practitioner)
    {
        ArgumentNullException.ThrowIfNull(practitioner);

        return new PractitionerDto
        {
            Id = practitioner.Id,
            Active = practitioner.Active ?? true,
            Name = practitioner.Name?.Select(n => n.ToDto()).ToList(),
            Gender = practitioner.Gender,
            BirthDate = practitioner.BirthDateElement?.ToDateTime(),
            Telecom = practitioner.Telecom?.Select(t => t.ToDto()).ToList(),
            Identifier = practitioner.Identifier?.Select(i => i.ToDto()).ToList(),
            LastUpdated = practitioner.Meta?.LastUpdated?.UtcDateTime
        };
    }
    public static Practitioner ToFhirPractitioner(this CreatePractitionerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

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
                LastUpdated = DateTime.UtcNow,
                VersionId = "1"
            }
        };
    }

    public static Practitioner ApplyUpdate(this Practitioner existing, UpdatePractitionerDto update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        if (update.WasSpecified(nameof(update.Active)) && update.Active.HasValue)
            existing.Active = update.Active.Value;

        if (update.WasSpecified(nameof(update.Name)))
            existing.Name = update.Name?.Select(n => n.ToFhirHumanName()).ToList() ?? [];

        if (update.WasSpecified(nameof(update.Gender)) && update.Gender.HasValue)
            existing.Gender = update.Gender;

        if (update.WasSpecified(nameof(update.BirthDate)))
            existing.BirthDateElement = update.BirthDate.ToFhirDate();

        if (update.WasSpecified(nameof(update.Telecom)))
            existing.Telecom = update.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? [];

        if (update.WasSpecified(nameof(update.Identifier)))
            existing.Identifier = update.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [];
        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTime.UtcNow;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}
