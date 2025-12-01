using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Dtos.Invoice;

public class InvoiceDetailDto
{
    public Guid Id { get; set; }

    // ============================
    // PACIENTE
    // ============================
    public string PatientIdFhir { get; set; } = null!;
    public string? PatientDisplay { get; set; }

    // ============================
    // TOTALES
    // ============================
    public decimal TotalOriginal { get; set; }
    public decimal AdjustmentTotal { get; set; }
    public decimal FinalTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }

    // ============================
    // ESTADO Y TIPO
    // ============================
    public InvoiceStatus Status { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }

    // ============================
    // SERIE Y NUMERO
    // ============================
    public long Number { get; set; }
    public Guid SerieId { get; set; }
    public string SerieName { get; set; }

    // ============================
    // ITEMS
    // ============================
    public List<InvoiceItemDetailDto> Items { get; set; } = new();

    // ============================
    // AUDITORÍA
    // ============================
    public DateTime CreatedDate { get; set; }
    public Guid CreatedById { get; set; }

    // ============================
    // FACTURA PADRE (si es hija)
    // ============================
    public Guid? ParentInvoiceId { get; set; }
    public string? ParentInvoiceNumber { get; set; }

    // ============================
    // FACTURAS HIJAS (notas)
    // ============================
    public List<InvoiceChildDto>? Children { get; set; }
    
    // ============================
    // RESUMEN DE NOTAS (Ajustes)
    // ============================
    public InvoiceNotesSummaryDto? NotesSummary { get; set; }
}

public class MinimalInvoiceDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public Guid SerieId { get; set; }
    public string SerieName { get; set; }
    public InvoiceStatus Status { get; set; }
    public InvoiceType InvoiceType { get; set; }
}
public class InvoiceItemDetailDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal TotalAmount { get; set; }
}


// =======================================
//        FACTURAS HIJAS (NOTAS)
// =======================================
public class InvoiceChildDto
{
    public Guid Id { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal FinalTotal { get; set; }
    public DateTime CreatedDate { get; set; }

    public long Number { get; set; }
    public Guid SerieId { get; set; }
}

// =======================================
//      RESUMEN DE NOTAS (CREDIT/DEBIT)
// =======================================
public class InvoiceNotesSummaryDto
{
    /// <summary>
    /// Total de notas de crédito aplicadas (montos negativos).
    /// </summary>
    public decimal TotalCreditNotes { get; set; }

    /// <summary>
    /// Total de notas de débito aplicadas (montos positivos).
    /// </summary>
    public decimal TotalDebitNotes { get; set; }

    /// <summary>
    /// Ajuste neto: sum(debito) - sum(credito).
    /// </summary>
    public decimal NetAdjustment => TotalDebitNotes - TotalCreditNotes;

    public int CountCredit { get; set; }
    public int CountDebit { get; set; }
}