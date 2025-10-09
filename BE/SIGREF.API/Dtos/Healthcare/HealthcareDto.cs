#nullable enable
using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Healthcare
{
    public class HealthcareDto
    {
        public string? Id { get; set; }

        public List<IdentifierDto>? Identifier { get; set; }

        public bool Active { get; set; } = true;

        public string Name { get; set; } = string.Empty;

        public string? Comment { get; set; } // descripción

        public string Abbreviation { get; set; } = string.Empty;

        public decimal? Cost { get; set; } = 0;

        public List<CodeableConceptDto>? Specialty { get; set; }

        public ReferenceDto? ProvidedBy { get; set; } // organización que presta el servicio

        public List<ReferenceDto>? Location { get; set; } // áreas donde se presta el servicio

        public DateTime? LastUpdated { get; set; }
    }
}
