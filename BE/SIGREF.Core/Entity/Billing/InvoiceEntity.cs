using SIGREF.Common.Types;
using SIGREF.Core.Entity.Cashier;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Billing;

/// <summary>
///     Representa el documento legal de facturación (Factura, Nota de Crédito/Débito).
///     Esta entidad centraliza la deuda del paciente y la relación con la sesión de caja.
/// </summary>
public class InvoiceEntity : BaseEntity
{
    #region Datos del Paciente (Snapshot de FHIR)

    /* IMPORTANTE: Se guardan estos datos por redundancia e integridad histórica.
       Si el servidor FHIR cambia el nombre o ID del paciente, la factura debe
       mantener los datos originales con los que se emitió.
    */

    /// <summary> Identificador técnico del recurso 'Patient' en el servidor FHIR. </summary>
    public string? PatientIdFhir { get; set; }

    /// <summary> Nombre completo del paciente tal como aparecía al momento de la venta. </summary>
    public string? PatientDisplay { get; set; }

    /// <summary>
    ///     El sistema de identificación (Namespace).
    ///     Ej: 'http://hl7.org/fhir/sid/dni' para Honduras.
    /// </summary>
    public string? PatientSystem { get; set; }

    /// <summary> El número de identidad real (DNI, Pasaporte, etc). </summary>
    public string? PatientValue { get; set; }

    #endregion

    #region Origen del Servicio

    /// <summary> Referencia al grupo de servicios/paquete si la venta fue por paquete. </summary>
    public string? ServiceGroupFhirId { get; set; }

    /// <summary> ID del servicio individual (si no es paquete). </summary>
    public Guid? SingleServiceId { get; set; }

    /// <summary>
    ///     Colección de ítems detallados que componen la factura.
    ///     <remarks> Solo cuando este es un paquete de servicios</remarks>
    /// </summary>
    public ICollection<InvoiceItemEntity> Items { get; set; } = new List<InvoiceItemEntity>();

    #endregion

    #region Numeración y Control

    /// <summary> ID de la serie de facturación (resolución de la entidad tributaria). </summary>
    public Guid SerieId { get; set; }

    public InvoiceSerieEntity? Serie { get; set; }

    /// <summary> Número secuencial único de la factura dentro de su serie. </summary>
    public long Number { get; set; }

    #endregion

    #region Totales y Finanzas

    /// <summary> Suma bruta de los ítems (Precio * Cantidad) ANTES de cualquier descuento o ajuste. </summary>
    public decimal TotalOriginal { get; set; }

    /// <summary> Descuento comercial aplicado al momento de la creación (500 -> 400 = 100 de descuento). </summary>
    public decimal InvoiceDiscount { get; set; }

    /// <summary>
    ///     Suma neta de ajustes posteriores (Notas de Crédito [-] o Débito [+]).
    ///     No debe confundirse con el descuento inicial.
    /// </summary>
    public decimal AdjustmentTotal { get; set; }

    /// <summary>
    ///     Monto final exigible: (TotalOriginal - InvoiceDiscount) + AdjustmentTotal.
    ///     Es el valor que el sistema espera que el cajero reciba.
    /// </summary>
    public decimal FinalTotal { get; set; }

    /// <summary>
    ///     Cantidad de dinero que el paciente ya ha pagado efectivamente.
    ///     <remarks> Esta posiblemente no se utilize, a menos se se permita el cobro a pagos</remarks>
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary> Saldo pendiente de pago: (FinalTotal - AmountPaid). </summary>
    public decimal AmountDue { get; set; }

    #endregion

    #region Estados y Clasificación

    /// <summary> Estado actual del flujo (Created, Paid, Cancelled, Refunded). </summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Created;

    /// <summary> Clasificación tributaria (Normal, Emergencia, Exento, etc). </summary>
    public InvoiceType InvoiceType { get; set; } = InvoiceType.Normal;

    /// <summary>
    ///     Forma principal en la que se pagó o pagará (Efectivo, Tarjeta, Mixto).
    ///     <remarks> Por defecto es cash a menos que se habiliten otro tipo de pagos</remarks>
    /// </summary>
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.Cash;

    #endregion

    #region Auditoría y Sesión

    /// <summary> Si esta es una Nota de Crédito/Débito, apunta a la factura original. </summary>
    public Guid? ParentInvoiceId { get; set; }

    public InvoiceEntity? ParentInvoice { get; set; }

    /// <summary> Sesión de caja en la que se creó o cobró esta factura. </summary>
    public Guid? CashierSessionId { get; set; }

    public CashierSessionEntity? CashierSession { get; set; }

    #endregion
}