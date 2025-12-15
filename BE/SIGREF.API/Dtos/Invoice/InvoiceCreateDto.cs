using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Dtos.Invoice;

/// <summary>
/// La rezon de pedir los Datos, es por que una orden de compra se realiza con los datos que EL USUARIO ESTA VISUALIZANDO EN EL MOMENTO
/// Dado esto, se necesita cargar la lista de Items de los servicios
/// Incluyendo si es un paquete - Los servicios contenidos por ese paquete como susprecios y sus IDS
/// </summary>
public class InvoiceCreateDto
{
    // ======== DATA DEL PACIENTE ========
    [Required]
    public string PatientIdFhir { get; set; } = null!;

    public string? PatientDisplay { get; set; }
    public string? PatientSystem { get; set; }
    public string? PatientValue { get; set; }

    // ======== SERVICIO O GRUPO ========
    public string? ServiceGroupFhirId { get; set; }
    public string? SingleServiceFhirId { get; set; }

    // ======== ÍTEMS ========
    [Required]
    public List<InvoiceItemCreateDto> Items { get; set; } = new();

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
    /// > 0 = pagaron parcialmente o totalmente.
    /// Solo aplica para Emergency; Normal y Exempt lo ignoran.
    /// </summary>
    public decimal? InitialPayment { get; set; }

}