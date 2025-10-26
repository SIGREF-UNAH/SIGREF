#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Patient
{
    public class PatientFilterDto : PagedFilterBase
    {
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public AdministrativeGender? Gender { get; set; }
    }
}
