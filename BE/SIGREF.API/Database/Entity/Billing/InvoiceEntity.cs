using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Billing;

// TODO
// Pasar los metodos de pago a enum de alguna manera y validar 
// Por el momento solo se aceptara cash en BE
[Table("invoices")]
public class InvoiceEntity : BaseEntity
{
    // ===============================
    //        FHIR: PACIENTE
    // ===============================

    [Required]
    [StringLength(64)]
    [Column("patient_id_fhir")]
    public string PatientIdFhir { get; set; } = null!;

    [StringLength(200)]
    [Column("patient_display")]
    public string? PatientDisplay { get; set; }

    [StringLength(50)]
    [Column("patient_system")]
    public string? PatientSystem { get; set; }

    [StringLength(200)]
    [Column("patient_value")]
    public string? PatientValue { get; set; }


    // ===============================
    //      USUARIO (Cajero)
    // ===============================

    [Required] [Column("user_id")] public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))] public UserLinkEntity? User { get; set; }


    // ===============================
    //      SERIE DE FACTURACIÓN
    // ===============================

    [Required] [Column("serie_id")] public Guid SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public InvoiceSerieEntity? Serie { get; set; }


    // ===============================
    //        DATOS DE FACTURA
    // ===============================

    [Required] [Column("number")] public long Number { get; set; }

    [Required] [Column("total_amount")] public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "LPS";

    [Required]
    [StringLength(30)]
    [Column("status")]
    public string Status { get; set; } = "Created";
    // Ej: Created, Paid, Cancelled, Refunded


    // ===============================
    //        MÉTODO DE PAGO
    // ===============================
    
    [Required]
    [Column("payment_method")]
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cash;
    // Ej: Cash, Card, Transfer, Mixed

    // ===============================
    //         TIPO DE FACTURA
    // ===============================

    [Required]
    [Column("invoice_type")]
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Normal;

    // ===============================
    //   REFERENCIA A FACTURA PADRE
    // ===============================

    [Column("parent_invoice_id")] public Guid? ParentInvoiceId { get; set; }

    [ForeignKey(nameof(ParentInvoiceId))] public InvoiceEntity? ParentInvoice { get; set; }


    // ===============================
    //    TURNO / SESIÓN DE CAJA
    // ===============================

    [Column("cashier_session_id")] public Guid? CashierSessionId { get; set; }

    [ForeignKey(nameof(CashierSessionId))] public CashierSessionEntity? CashierSession { get; set; }
}