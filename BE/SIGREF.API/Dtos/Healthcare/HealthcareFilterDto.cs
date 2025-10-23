#nullable enable
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Healthcare
{
    public class HealthcareFilterDto : PagedFilterBase
    {
        public string? Name { get; set; }

        public bool? Active { get; set; }

        public string? Specialty { get; set; }

        public string? ProvidedBy { get; set; }

        public string? Location { get; set; }
    }
}
