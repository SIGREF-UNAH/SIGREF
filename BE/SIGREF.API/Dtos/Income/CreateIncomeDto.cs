using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;

namespace SIGREF.API.Dtos.Income
{
    public class CreateIncomeDto
    {
        public List<IdentifierDto>? Identifier { get; set; }

        public bool Active { get; set; } = true;

        [Required(ErrorMessage = "El número de recibo es requerido")]
        [MaxLength(50, ErrorMessage = "El número de recibo no puede exceder los 50 caracteres")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es requerida")]
        [DataType(DataType.Date)]
        public DateTime IncomeDate { get; set; } = DateTime.Now;

        [MaxLength(255, ErrorMessage = "La descripción no puede exceder los 255 caracteres")]
        public string? Description { get; set; }

        // Referencias a entidades FHIR
        [Required(ErrorMessage = "La referencia al paciente es requerida")]
        public ReferenceDto Patient { get; set; } = null!;

        public ReferenceDto? Practitioner { get; set; }

        [Required(ErrorMessage = "La referencia al servicio médico es requerida")]
        public ReferenceDto Healthcare { get; set; } = null!;

        public ReferenceDto? Location { get; set; }

        public ReferenceDto? Organization { get; set; }
    }

    public class CreateCounterpartIncomeDto : CreateIncomeDto
    {
        [Required(ErrorMessage = "El ID del ingreso original es requerido para una contrapartida")]
        public string OriginalIncomeId { get; set; } = null!;

        [Required(ErrorMessage = "La razón de la contrapartida es requerida")]
        [MaxLength(255, ErrorMessage = "La razón no puede exceder los 255 caracteres")]
        public string Reason { get; set; } = null!;
    }
}