namespace SIGREF.Common.Types;


/// <summary>
/// Estados posibles de una factura.
/// Estos valores se almacenan como TEXTO en la base de datos 
/// (Created, Paid, Cancelled, Refunded).
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// La factura fue creada pero aún no está pagada.
    /// Estado inicial.
    /// </summary>
    Created,

    /// <summary>
    /// La factura fue pagada completamente.
    /// No pueden editarse montos; solo puede reembolsarse.
    /// </summary>
    Paid,

    /// <summary>
    /// La factura fue anulada completamente.
    /// No genera efectos contables.
    /// </summary>
    Cancelled,

    /// <summary>
    /// La factura tiene un reembolso parcial o total.
    /// Se usa para notas de crédito o devoluciones.
    /// </summary>
    Refunded
}

