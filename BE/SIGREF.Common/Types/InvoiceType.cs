namespace SIGREF.Common.Types;

/// <summary>
///     Tipo de factura emitida.
///     Controla el flujo de cobro, auditoría y comportamiento del total.
/// </summary>
public enum InvoiceType
{
    /// <summary>
    ///     Factura normal.
    ///     Se cobra al momento, registra pago y comportamiento habitual.
    /// </summary>
    Normal,

    /// <summary>
    ///     Factura de emergencia.
    ///     El servicio se registra inmediatamente, pero:
    ///     - Puede que no se haya cobrado aún.
    ///     - El paciente puede pagar después.
    ///     - Se conoce quién registró la factura (CreatedById).
    ///     Estado financiero queda pendiente.
    /// </summary>
    Emergency,

    /// <summary>
    ///     Factura exonerada.
    ///     El paciente no paga.
    ///     Se Asume un Descuento del 100%
    ///     El monto se registra como 0.
    ///     Se mantiene trazabilidad del servicio brindado para control y auditoría.
    /// </summary>
    Exempt,

    /// Nota de crédito: monto NEGATIVO, devuelve dinero.
    /// Se usa cuando se cobró de más o se devuelve parte o todo.
    CreditNote,

    /// Nota de débito: monto POSITIVO, cobra dinero faltante.
    /// Se usa cuando se cobró de menos y falta saldo.
    DebitNote
}