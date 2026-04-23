using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIGREF.Common.Types;

namespace SIGREF.API.Dtos.Invoice;

/// <summary>
/// DTO para crear una factura o nota de crédito/débito.
///
/// MODELO DE DESCUENTO:
/// El descuento aplica a TODA la factura, no a ítems individuales.
/// Se guarda en InvoiceEntity.InvoiceDiscount y se resta al FinalTotal.
/// FinalTotal = TotalOriginal - InvoiceDiscount + AdjustmentTotal
/// </summary>
public class InvoiceCreateDto
{
    // ======== DATOS DEL PACIENTE ========
    public string? PatientIdFhir { get; set; }

    public string? PatientDisplay { get; set; }
    public string? PatientSystem { get; set; }
    public string? PatientValue { get; set; }

    // ======== SERVICIO O GRUPO ========
    // Solo uno de los dos debe venir informado.
    public string? ServiceGroupFhirId { get; set; }
    public string? SingleServiceFhirId { get; set; }

    // ======== ÍTEMS ========
    [Required]
    public List<InvoiceItemCreateDto> Items { get; set; } = new();

    // ======== DESCUENTO GLOBAL ========
    /// <summary>
    /// Descuento aplicado a toda la factura (monto absoluto, no porcentaje).
    /// Null o 0 = sin descuento.
    /// No puede superar el TotalOriginal; el servidor valida esto.
    /// </summary>
    public decimal? InvoiceDiscount { get; set; } 

    // ======== METADATOS ========
    [Required]
    [JsonPropertyName("invoice_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceType InvoiceType { get; set; }

    [Required]
    [JsonPropertyName("payment_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentMethodType PaymentMethod { get; set; }

    public Guid SerieId { get; set; }
    public long SerieNumber { get; set; }

    public Guid? ParentInvoiceId { get; set; }   // para notas de crédito/débito

    /// <summary>
    /// Pago inicial al momento de crear la factura.
    /// Null o 0 = NO pagaron nada.
    /// > 0  = pago parcial o total al crear.
    /// Solo aplica para Emergency; Normal y Exempt lo ignoran.
    /// </summary>
    public decimal? InitialPayment { get; set; }
}