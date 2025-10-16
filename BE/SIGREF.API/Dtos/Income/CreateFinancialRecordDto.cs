using System.ComponentModel.DataAnnotations;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Income
{
    public class CreateFinancialRecordDto
    {
        public List<IdentifierDto> Identifiers { get; set; }

        [Required(ErrorMessage ="El numero de recibo es requerido")]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required(ErrorMessage ="EL monto es requerido")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage ="La fecha del ingreso es requerida ")]
        public DateTime IncomeDate {  get; set; } = DateTime .Now;
        public string Description { get; set; }

        [Required(ErrorMessage =("El paciente es requerido"))]
        public ReferenceDto Patient {  get; set; }

        [Required(ErrorMessage ="El servicio medico es requerido ")]
        public ReferenceDto Healthcare {  get; set; }   

        public bool ValideteAmount (decimal serviceCost)
        {
            return Amount <=  serviceCost;
        }
    }

    public class CreateCountrpartFinancialRecordDto : CreateFinancialRecordDto
    {
        [Required(ErrorMessage = "El ID del ingreso Original es requerida")]
        public string OriginalIncomeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razon de la contrapartida es requerida")]
        [StringLength(200, ErrorMessage ="La razon no puede exceder los 200 caracteres")]
        public string Reason { get; set; } = string.Empty ;
    }
}
