using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Constants;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Services.Billing;

namespace SIGREF.API.Controllers.Invoices;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoiceController(IInvoiceService service)
    {
        _service = service;
    }

    // ============================================
    // CREAR FACTURA
    // ============================================
    [HttpPost]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin}")]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceCreateDto dto)
    {
        var result = await _service.CreateInvoiceAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // OBTENER FACTURA POR ID
    // ============================================
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetInvoiceById(
        Guid id,
        [FromQuery] bool includeNotes = true,
        [FromQuery] int notesPage = 1,
        [FromQuery] int notesPageSize = 10)
    {
        var result = await _service.GetInvoiceByIdAsync(id, includeNotes, notesPage, notesPageSize);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // LISTAR FACTURAS
    // ============================================
    [HttpGet]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterDto filter)
    {
        var result = await _service.GetInvoicesAsync(filter);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // CANCELAR FACTURA
    // ============================================
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{RolesConstants.admin},")]
    public async Task<IActionResult> CancelInvoice(Guid id)
    {
        var result = await _service.CancelInvoiceAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // PAGAR FACTURA
    // ============================================
    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> MarkAsPaid(
        Guid id,
        [FromBody] decimal amountPaid)
    {
        var result = await _service.MarkAsPaidAsync(id, amountPaid);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // CREAR NOTA DE CRÉDITO / DÉBITO
    // ============================================
    [HttpPost("{parentId:guid}/notes/{noteType}")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> CreateNote(
        Guid parentId,
        InvoiceType noteType,
        [FromBody] InvoiceCreateDto dto)
    {
        var result = await _service.CreateNoteAsync(parentId, dto, noteType);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // VERIFICAR SI TIENE NOTAS HIJAS
    // ============================================
    [HttpGet("{id:guid}/child-notes")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> HasChildNotes(Guid id)
    {
        var result = await _service.HasChildNotesAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // RECALCULAR TOTALES
    // ============================================
    [HttpPost("{id:guid}/recalculate")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> RecalculateTotals(Guid id)
    {
        var result = await _service.RecalculateInvoiceTotalsAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    
    // SUMMARY DE NOTAS (CREDIT/DEBIT)
    [HttpGet("{id:guid}/notes-summary")]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetNotesSummary(Guid id)
    {
        var result = await _service.GetNotesSummaryAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}