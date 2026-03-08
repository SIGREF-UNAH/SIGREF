using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Invoice;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;


namespace SIGREF.API.Services.Billing;

public interface IInvoiceService
{
    // ================================
    //      CREAR FACTURAS
    // ================================
    Task<ResponseDto<InvoiceDetailDto>> CreateInvoiceAsync(InvoiceCreateDto dto);

    // ================================
    //      OBTENER FACTURAS
    // ================================
    Task<ResponseDto<InvoiceDetailDto?>> GetInvoiceByIdAsync(
        Guid id,
        bool includeNotes = true,
        int notesPage = 1,
        int notesPageSize = 10);


    // ================================
    //      LISTAR FACTURAS
    // ================================
    Task<ResponseDto<PagedResultDto<InvoiceGetDto>>> GetInvoicesAsync(InvoiceFilterDto filter);

    // ================================
    //   ESTADO, PAGO Y CANCELACIÓN
    // ================================
    Task<ResponseDto<InvoiceDetailDto>> CancelInvoiceAsync(Guid id);
    Task<ResponseDto<InvoiceDetailDto>> MarkAsPaidAsync(Guid id, decimal amountPaid);

    // ================================
    //     NOTAS DE CRÉDITO / DÉBITO
    // ================================
    Task<ResponseDto<InvoiceDetailDto>> CreateNoteAsync(Guid parentInvoiceId, InvoiceCreateDto dto,InvoiceType noteType);

    /// <summary>
    /// Verifica si una factura tiene notas hijas.
    /// </summary>
    Task<ResponseDto<List<MinimalInvoiceDto>>> HasChildNotesAsync(Guid invoiceId);

    /// <summary>
    /// Recalcula totales de la factura PADRE tomando en cuenta notas hijas.
    /// (credit notes / debit notes)
    /// </summary>
    Task<ResponseDto<bool>> RecalculateInvoiceTotalsAsync(Guid invoiceId);

    // ================================
    //   SUMMARY DE NOTAS (Ajustes)
    // ================================
    /// <summary>
    /// Obtiene un resumen de notas de crédito/débito para una factura.
    /// </summary>
    Task<ResponseDto<InvoiceNotesSummaryDto>> GetNotesSummaryAsync(Guid invoiceId);
}

