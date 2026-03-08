using System.Text.Json.Serialization;
using SIGREF.API.Database.Entity.common;
using SIGREF.Common.Types;

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus Status { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceType InvoiceType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
    // ===== SUMMARY FINANCIERO (SIEMPRE COMPLETO) =====
    public decimal TotalCreditNotes { get; set; }
    public decimal TotalDebitNotes { get; set; }
    public int CountCredit { get; set; }
    public int CountDebit { get; set; }

    // ===== INFORMACIÓN DE PAGINACIÓN =====
    public int TotalNotes { get; set; }        // Total de notas (credit + debit)
    public int CurrentPage { get; set; }        // Página actual
    public int PageSize { get; set; }           // Tamaño de página
    public int TotalPages { get; set; }         // Total de páginas
    
    // ===== PROPIEDADES COMPUTADAS (OPCIONALES) =====
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    
    // Balance neto de todas las notas
    public decimal NetAdjustment => TotalCreditNotes + TotalDebitNotes;
}


