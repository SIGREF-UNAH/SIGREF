#nullable enable
using Hl7.Fhir.Model;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Practitioner
{
    public class PractitionerFilterDto : PagedFilterBase
    {
        public string? Name { get; set; }

        public bool? Active { get; set; }

        public AdministrativeGender? Gender { get; set; }
    }
}
