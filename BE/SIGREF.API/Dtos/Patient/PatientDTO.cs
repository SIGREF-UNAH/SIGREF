using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Patient
{
    public class PatientDTO
    {
        public string? Id { get; set; }
        public bool Active { get; set; }
        public List<HumanNameDto>? Name { get; set; }
        public AdministrativeGender? Gender { get; set; } // "male", "female", "other", "unknown"
        public DateTime? BirthDate { get; set; }
        public List<ContactPointDto>? Telecom { get; set; }
        public List<AddressDto>? Address { get; set; }
        public List<IdentifierDto>? Identifier { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
