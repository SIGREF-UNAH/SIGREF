#nullable enable
using Hl7.Fhir.Model;

namespace SIGREF.API.Dtos.Practitioner
{
    public class PractitionerFilterDto
    {
        public string? Name { get; set; }

        public bool? Active { get; set; }

        public AdministrativeGender? Gender { get; set; }

        // Paginación
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
