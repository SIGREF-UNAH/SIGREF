#nullable enable
using Hl7.Fhir.Model;
using Humanizer;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Patient;
using SIGREF.API.Extensions.Common;

namespace SIGREF.API.Extensions
{
    public static class PatientExtensions
    {


        public static PatientDTO ToDto(this Patient patient)
        {
            return new PatientDTO
            {
                Id = patient.Id,
                Active = patient.Active ?? true,
                Name = patient.Name?.Select(n => n.ToDto()).ToList(),
                Gender = patient.Gender?.ToString().ToLowerInvariant(),
                BirthDate = patient.BirthDateElement?.ToDateTime(),
                Telecom = patient.Telecom?.Select(t => t.ToDto()).ToList(),
                Address = patient.Address?.Select(a => a.ToDto()).ToList(),
                Identifier = patient.Identifier?.Select(i => i.ToDto()).ToList(),
                LastUpdated = patient.Meta?.LastUpdated?.DateTime
            };
        }

        public static Patient ToFhirPatient(this CreatePatientDto dto)
        {
            return new Patient
            {
                Active = dto.Active,
                Name = dto.Name?.Select(n => n.ToFhirHumanName()).ToList() ?? [],
                Gender = dto.Gender,
                BirthDateElement = dto.BirthDate.ToFhirDate(),
                Telecom = dto.Telecom?.Select(t => t.ToFhirContactPoint()).ToList() ?? new List<ContactPoint>(),
                Address = dto.Address?.Select(a => a.ToFhirAddress()).ToList() ?? new List<Address>(),
                Identifier = dto.Identifier?.Select(i => i.ToFhirIdentifier()).ToList() ?? new List<Identifier>(),
                Meta = new Meta
                {
                    LastUpdated = DateTimeOffset.Now,
                    VersionId = "1"
                }
            };
        }

        public static Patient ApplyUpdate(this Patient existing, UpdatePatientDto update)
        {
            if (update.Active.HasValue) existing.Active = update.Active.Value;
            if (update.Name != null) existing.Name = update.Name.Select(n => n.ToFhirHumanName()).ToList();
            if (!string.IsNullOrEmpty(update.Gender?.ToString())) existing.Gender = update.Gender;
            if (update.BirthDate.HasValue) existing.BirthDateElement = update.BirthDate.ToFhirDate();
            if (update.Telecom != null) existing.Telecom = update.Telecom.Select(t => t.ToFhirContactPoint()).ToList();
            if (update.Address != null) existing.Address = update.Address.Select(a => a.ToFhirAddress()).ToList();
            if (update.Identifier != null) existing.Identifier = update.Identifier.Select(i => i.ToFhirIdentifier()).ToList();

            existing.Meta ??= new Meta();
            existing.Meta.LastUpdated = DateTimeOffset.Now;
            existing.Meta.VersionId = FhirInfrastructureExtensions.IncrementVersion(existing.Meta.VersionId);

            return existing;
        }

    }
}
