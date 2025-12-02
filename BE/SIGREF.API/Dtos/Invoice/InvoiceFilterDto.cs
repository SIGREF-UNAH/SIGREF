using System.Text.Json.Serialization;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Common;

namespace SIGREF.API.Dtos.Invoice;

// =====================================================================
// ACTUALIZAR InvoiceFilterDto PARA INCLUIR ORDENAMIENTO (OPCIONAL)
// =====================================================================

public class InvoiceFilterDto : PagedFilterBase
{
    // Filtros existentes...
    public string? Search { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceType? InvoiceType { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus? Status { get; set; }
    
    public Guid? SerieId { get; set; }
    public long? Number { get; set; }
    public string? PatientIdFhir { get; set; }
    public string? PatientDisplay { get; set; }
    public Guid? CashierSessionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public bool? OnlyInvoices { get; set; }
    public bool? OnlyNotes { get; set; }

    // ============================
    // ORDENAMIENTO (OPCIONAL)
    // ============================
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceSortField? SortBy { get; set; } = InvoiceSortField.CreatedDate;
    public bool SortDescending { get; set; } = true;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceSortField
{
    CreatedDate,
    Number,
    PatientDisplay,
    FinalTotal,
    Status,
    InvoiceType
}