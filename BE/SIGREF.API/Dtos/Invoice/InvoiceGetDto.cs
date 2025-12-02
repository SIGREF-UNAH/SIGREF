using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Dtos.Invoice;

public class InvoiceGetDto
{
    public Guid Id { get; set; }

    public string? PatientDisplay { get; set; }

    public InvoiceStatus Status { get; set; }

    public InvoiceType InvoiceType { get; set; }

    public decimal TotalOriginal { get; set; }

    public decimal AdjustmentTotal { get; set; }

    public decimal FinalTotal { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal AmountDue { get; set; }

    public long Number { get; set; }

    public DateTime CreatedDate { get; set; }
}