using SIGREF.API.Dtos.Common;
using System.ComponentModel.DataAnnotations;
namespace SIGREF.API.Dtos.Income
{
    public class FinancialRecordDto
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string TransactionNumber { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string ServiceName { get; set; } = string.Empty;

        public string ServiceId { get; set; }

        [Required]
        public string PatientName { get ; set; } = string.Empty;

        public string PatientId { get; set; }

        public string PractionerId { get; set; }

        public string PractitionerName {  get; set; }

        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string Notes {  get; set; }
        
        public string CashierName { get; set; }

        public string  CashierId { get; set; }

        public bool IsVoided {  get; set; }

        public string VoidReason { get; set; }

        public string VoidedBy { get; set; }

        public DateTime VoidDate { get; set; }

        public string RelatedRecordId { get; set; }

    }
}
