#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions;

public static class PatientExtensions
{
    private const string NationalityExtensionUrl = "http://hl7.org/fhir/StructureDefinition/patient-nationality";

    public static PatientDto ToDto(this Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);

        return new PatientDto
        {
            Id = patient.Id,
            Active = patient.Active ?? true,
            Name = patient.Name?.Select(n => n.ToDto()).ToList(),
            Gender = patient.Gender,
            BirthDate = patient.BirthDateElement?.ToDateTime(),
            MaritalStatus = patient.MaritalStatus?.ToCodeableConceptDto(),
            Telecom = patient.Telecom?.Select(t => t.ToDto()).ToList(),
            Address = patient.Address?.Select(a => a.ToDto()).ToList(),
            Identifier = patient.Identifier?.Select(i => i.ToDto()).ToList(),
            LastUpdated = patient.Meta?.LastUpdated?.UtcDateTime,
            Extension = patient.Extension?.Select(e => e.ToDto()).ToList()
        };
    }

    public static Patient ToFhirPatient(this CreatePatientDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var patient = new Patient
        {
            Active = dto.Active,
            Name = dto.Name?.Select(n => n.ToFhirHumanName()).ToList() ?? [],
            Gender = dto.Gender,
            BirthDateElement = dto.BirthDate.ToFhirDate(),
            MaritalStatus = dto.MaritalStatus?.ToFhirCodeableConcept(),
            Telecom = dto.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? [],
            Address = dto.Address?.Select(a => a.ToFhirAddress()).ToList() ?? [],
            Identifier = dto.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [],
            Extension = dto.Extension?.Select(e => e.ToFhirExtension()).ToList() ?? [],
            Meta = new Meta
            {
                LastUpdated = DateTime.UtcNow,
                VersionId = "1"
            }
        };

        // Validar que tenga la extensión de nacionalidad
        if (!patient.Extension.Any(e => e.Url == NationalityExtensionUrl))
        {
            throw new ArgumentException("El paciente debe tener una extensión de nacionalidad");
        }

        return patient;
    }

    public static Patient ApplyUpdate(this Patient existing, UpdatePatientDto update)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(update);

        if (update.WasSpecified(nameof(update.Active)) && update.Active.HasValue) existing.Active = update.Active.Value;
        if (update.WasSpecified(nameof(update.Name))) existing.Name = update.Name?.Select(n => n.ToFhirHumanName()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Gender)) && update.Gender.HasValue) existing.Gender = update.Gender;
        if (update.WasSpecified(nameof(update.BirthDate))) existing.BirthDateElement = update.BirthDate.ToFhirDate();
        if (update.WasSpecified(nameof(update.MaritalStatus))) existing.MaritalStatus = update.MaritalStatus?.ToFhirCodeableConcept();
        if (update.WasSpecified(nameof(update.Telecom))) existing.Telecom = update.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Address))) existing.Address = update.Address?.Select(a => a.ToFhirAddress()).ToList() ?? [];
        if (update.WasSpecified(nameof(update.Identifier))) existing.Identifier = update.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? [];

        // Manejar extensiones
        if (update.WasSpecified(nameof(update.Extension)))
            existing.Extension = update.Extension?.Select(e => e.ToFhirExtension()).ToList() ?? [];

        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTime.UtcNow;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}
