#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SIGREF.API.Dtos.Healthcare
{
    public class HealthcareFilterDto
    {
        public string? Name { get; set; }

        public bool? Active { get; set; }

        public string? Specialty { get; set; }

        public string? ProvidedBy { get; set; }

        public string? Location { get; set; }

        // Paginación
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
