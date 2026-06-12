using SIGREF.API.Dtos.Invoice;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;


namespace SIGREF.API.Services.Billing;

public interface IInvoiceService
{
    // ================================
    //      CREAR FACTURAS
    // ================================
    Task<InvoiceDetailDto> CreateInvoiceAsync(InvoiceCreateDto dto);

    // ================================
    //      OBTENER FACTURAS
    // ================================
    Task<InvoiceDetailDto> GetInvoiceByIdAsync(
        Guid id,
        GetInvoiceParameters parameters);


    // ================================
    //      LISTAR FACTURAS
    // ================================
    Task<PagedResultDto<InvoiceGetDto>> GetInvoicesAsync(InvoiceFilterDto filter);

    // ================================
    //   ESTADO, PAGO Y CANCELACIÓN
    // ================================
    Task<InvoiceDetailDto> CancelInvoiceAsync(Guid id);
    Task<InvoiceDetailDto> MarkAsPaidAsync(Guid id, decimal amountPaid);

    // ================================
    //     NOTAS DE CRÉDITO / DÉBITO
    // ================================
    Task<InvoiceDetailDto> CreateNoteAsync(Guid parentInvoiceId, InvoiceCreateDto dto,InvoiceType noteType);

    /// <summary>
    /// Verifica si una factura tiene notas hijas.
    /// </summary>
    Task<List<MinimalInvoiceDto>> HasChildNotesAsync(Guid invoiceId);

    /// <summary>
    /// Recalcula totales de la factura PADRE tomando en cuenta notas hijas.
    /// (credit notes / debit notes)
    /// </summary>
    Task RecalculateInvoiceTotalsAsync(Guid invoiceId);

    // ================================
    //   SUMMARY DE NOTAS (Ajustes)
    // ================================
    /// <summary>
    /// Obtiene un resumen de notas de crédito/débito para una factura.
    /// </summary>
    Task<InvoiceNotesSummaryDto> GetNotesSummaryAsync(Guid invoiceId);
}

