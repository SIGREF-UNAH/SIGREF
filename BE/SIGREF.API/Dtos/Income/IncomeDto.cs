using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Income
{
    public class IncomeDto
    {
        public string? Id { get; set; }

        public List<IdentifierDto>? Identifier { get; set; }

        public bool Active { get; set; } = true;

        public string ReceiptNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime IncomeDate { get; set; }

        public string? Description { get; set; }

        public ReferenceDto? Patient { get; set; }

        public ReferenceDto? Practitioner { get; set; }

        public ReferenceDto? Healthcare { get; set; }

        public ReferenceDto? Location { get; set; }

        public ReferenceDto? Organization { get; set; }
        public string? CounterpartId { get; set; }

        public bool IsCounterpart { get; set; } = false;

        public DateTime? LastUpdated { get; set; }
    }
}
