using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Services.Billing;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;
using Swashbuckle.AspNetCore.Annotations;

namespace SIGREF.API.Controllers.Invoices;

[ApiController]
[Route("api/invoices")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[SwaggerTag("Facturas - Gestión de Facturación")]
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
    [SwaggerOperation(
        OperationId = "CreateInvoice",
        Summary = "Crear factura",
        Description = "Crea una nueva factura en el sistema con los detalles proporcionados.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ResponseDto<InvoiceDetailDto>))]
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
    [SwaggerOperation(
        OperationId = "GetInvoiceById",
        Summary = "Obtener factura por ID",
        Description = "Recupera el detalle completo de una factura específica utilizando su identificador único.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<InvoiceDetailDto?>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetInvoiceById(
        Guid id,
        [FromQuery] GetInvoiceParameters parameters)
    {
        var result = await _service.GetInvoiceByIdAsync(id, parameters);
        return StatusCode(result.StatusCode, result);
    }

    // ============================================
    // LISTAR FACTURAS
    // ============================================
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetInvoiceList",
        Summary = "Listar facturas",
        Description = "Obtiene una lista paginada y filtrada de las facturas registradas en el sistema.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<PagedResultDto<InvoiceGetDto>>))]
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
    [SwaggerOperation(
        OperationId = "CreateInvoiceByIdCancellation",
        Summary = "Crea una cancelacion factura",
        Description = "Anula o cancela una factura existente en el sistema mediante su identificador.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<InvoiceDetailDto>))]
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
    [SwaggerOperation(
        OperationId = "CreateInvoiceMarkAsPaid",
        Summary = "Pagar factura",
        Description = "Marca una factura como pagada registrando el monto abonado.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<InvoiceDetailDto>))]
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
    [SwaggerOperation(
        OperationId = "CreateInvoiceNote",
        Summary = "Crear nota de crédito/débito",
        Description = "Genera una nota de crédito o débito asociada a una factura padre específica.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(ResponseDto<InvoiceDetailDto>))]
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
    [SwaggerOperation(
        OperationId = "GetInvoiceHasChildNotes",
        Summary = "Verificar notas hijas",
        Description = "Comprueba y lista las notas de crédito o débito asociadas a una factura en particular.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<List<MinimalInvoiceDto>>))]
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
    [SwaggerOperation(
        OperationId = "CreateInvoiceByIdRecalculation",
        Summary = "Na",
        Description = "NA",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<bool>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> RecalculateTotals(Guid id)
    {
        var result = await _service.RecalculateInvoiceTotalsAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    
    // SUMMARY DE NOTAS (CREDIT/DEBIT)
    [HttpGet("{id:guid}/notes-summary")]
    [SwaggerOperation(
        OperationId = "GetInvoiceRecalculateNotesSummary",
        Summary = "Recalcular totales de factura",
        Description = "Vuelve a calcular los totales de una factura específica.",
        Tags = new[] { "Invoice" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(ResponseDto<InvoiceNotesSummaryDto>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetNotesSummary(Guid id)
    {
        var result = await _service.GetNotesSummaryAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
