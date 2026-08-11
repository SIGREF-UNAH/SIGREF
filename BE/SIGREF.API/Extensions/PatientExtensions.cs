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
            LastUpdated = patient.Meta?.LastUpdated?.DateTime,
            Extension = patient.Extension?.Select(e => e.ToDto()).ToList()
        };
    }

    public static Patient ToFhirPatient(this CreatePatientDto dto)
    {
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
        if (update.Active.HasValue) existing.Active = update.Active.Value;
        if (update.Name != null) existing.Name = update.Name.Select(n => n.ToFhirHumanName()).ToList();
        if (!string.IsNullOrEmpty(update.Gender?.ToString())) existing.Gender = update.Gender;
        if (update.BirthDate.HasValue) existing.BirthDateElement = update.BirthDate.ToFhirDate();
        if (update.MaritalStatus != null) existing.MaritalStatus = update.MaritalStatus.ToFhirCodeableConcept();
        if (update.Telecom != null) existing.Telecom = update.Telecom.Select(t => t.ToFhirContactPoint()).ToList();
        if (update.Address != null) existing.Address = update.Address.Select(a => a.ToFhirAddress()).ToList();
        if (update.Identifier != null) existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();

        // Manejar extensiones
        if (update.Extension != null)
        {
            existing.Extension = update.Extension.Select(e => e.ToFhirExtension()).ToList();
        }

        existing.Meta ??= new Meta();
        existing.Meta.LastUpdated = DateTime.UtcNow;
        existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

        return existing;
    }
}
