using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Database.Entity.common;
using SIGREF.Common.Types;

namespace SIGREF.API.Database.Entity.Billing;

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


    // ======================================================
    //  MODELO DE FACTURACIÓN: SERVICIO / GRUPO / MULTIPLE
    // ======================================================

    // Si se factura un grupo entero desde FHIR (solo referencia)
    [Column("service_group_fhir_id")] public string? ServiceGroupFhirId { get; set; }

    // Si se factura únicamente un servicio individual
    [Column("single_service_id")] public Guid? SingleServiceId { get; set; }

    // Relación con items (siempre se usan para congelar historial)
    public ICollection<InvoiceItemEntity> Items { get; set; } = new List<InvoiceItemEntity>();


    // ===============================
    //      SERIE DE FACTURACIÓN
    // ===============================

    [Required][Column("serie_id")] public Guid SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public InvoiceSerieEntity? Serie { get; set; }

    [Required][Column("number")] public long Number { get; set; }


    // ===========================================================
    //      TOTALES PRECALCULADOS (Optimización para 1–3M + registros)
    // ===========================================================

    /// <summary>
    /// Total original basado en invoice_items. Congelado al crear.
    /// </summary>
    [Required]
    [Column("total_original")]
    public decimal TotalOriginal { get; set; }

    /// <summary>
    /// Suma neta de notas de crédito/débito.
    /// DebitNote = positivo.
    /// CreditNote = negativo.
    /// </summary>
    [Required]
    [Column("adjustment_total")]
    public decimal AdjustmentTotal { get; set; }

    /// <summary>
    /// Total final luego de ajustes.
    /// FinalTotal = TotalOriginal + AdjustmentTotal
    /// </summary>
    [Required]
    [Column("final_total")]
    public decimal FinalTotal { get; set; }

    /// <summary>
    /// Monto pagado hasta ahora.
    /// </summary>
    [Required]
    [Column("amount_paid")]
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Saldo pendiente: FinalTotal - AmountPaid.
    /// </summary>
    [Required]
    [Column("amount_due")]
    public decimal AmountDue { get; set; }


    // ===============================
    //        ESTADO DE FACTURA
    // ===============================
    [Required]
    [Column("status", TypeName = "varchar(30)")]
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Created;

    /// Estados:
    /// - Created   : Factura creada, pendiente de pago.
    /// - Paid      : Factura pagada completamente.
    /// - Cancelled : Factura anulada (sin efectos contables).
    /// - Refunded  : Factura reembolsada parcial o totalmente.


    // ===============================
    //        TIPO DE FACTURA
    //  (Normal, Emergency, Exempt, CreditNote, DebitNote)
    // ===============================

    [Required]
    [Column("invoice_type", TypeName = "varchar(30)")]
    [EnumDataType(typeof(InvoiceType))]
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Normal;

    // ===============================
    //        MÉTODO DE PAGO
    // ===============================

    [Required]
    [Column("payment_method")]
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cash;
    // Cash, Card, Transfer, Mixed


    // ===============================
    //   RELACIÓN PADRE (AJUSTES)
    // ===============================

    [Column("parent_invoice_id")]
    public Guid? ParentInvoiceId { get; set; }

    [ForeignKey(nameof(ParentInvoiceId))]
    public InvoiceEntity? ParentInvoice { get; set; }


    // ===============================
    //     SESIÓN DE CAJA
    // ===============================

    [Column("cashier_session_id")]
    public Guid? CashierSessionId { get; set; }

    [ForeignKey(nameof(CashierSessionId))]
    public CashierSessionEntity? CashierSession { get; set; }
}