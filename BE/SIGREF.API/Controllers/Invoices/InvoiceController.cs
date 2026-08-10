using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Services.Billing;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Types;

namespace SIGREF.API.Controllers.Invoices;

/// <summary>Gestiona facturas, pagos, cancelaciones y notas asociadas.</summary>
/// <remarks>Dominio SIGREF: las facturas se almacenan en el modelo de negocio local y pueden referenciar identificadores FHIR.</remarks>
[ApiController]
[Route("api/invoices")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Tags("Facturas - Gestión de Facturación")]
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
    [EndpointName("CreateInvoice")]
    [EndpointSummary("Crear factura")]
    [EndpointDescription("Crea una nueva factura en el sistema con los detalles proporcionados.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(InvoiceDetailDto))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin}")]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceCreateDto dto)
    {
        var result = await _service.CreateInvoiceAsync(dto);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, result);
    }

    // ============================================
    // OBTENER FACTURA POR ID
    // ============================================
    [HttpGet("{id:guid}")]
    [EndpointName("GetInvoiceById")]
    [EndpointSummary("Obtener factura por ID")]
    [EndpointDescription("Recupera el detalle completo de una factura específica utilizando su identificador único.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(InvoiceDetailDto))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetInvoiceById(
        Guid id,
        [FromQuery] GetInvoiceParameters parameters)
    {
        var result = await _service.GetInvoiceByIdAsync(id, parameters);
        return Ok(result);
    }

    // ============================================
    // LISTAR FACTURAS
    // ============================================
    [HttpGet]
    [EndpointName("GetInvoiceList")]
    [EndpointSummary("Listar facturas")]
    [EndpointDescription("Obtiene una lista paginada y filtrada de las facturas registradas en el sistema.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(PagedResultDto<InvoiceGetDto>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterDto filter)
    {
        var result = await _service.GetInvoicesAsync(filter);
        return Ok(result);
    }

    // ============================================
    // CANCELAR FACTURA
    // ============================================
    [HttpPost("{id:guid}/cancel")]
    [EndpointName("CreateInvoiceByIdCancellation")]
    [EndpointSummary("Crea una cancelacion factura")]
    [EndpointDescription("Anula o cancela una factura existente en el sistema mediante su identificador.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(InvoiceDetailDto))]
    [Authorize(Roles = $"{RolesConstants.admin},")]
    public async Task<IActionResult> CancelInvoice(Guid id)
    {
        var result = await _service.CancelInvoiceAsync(id);
        return Ok(result);
    }

    // ============================================
    // PAGAR FACTURA
    // ============================================
    [HttpPost("{id:guid}/pay")]
    [EndpointName("CreateInvoiceMarkAsPaid")]
    [EndpointSummary("Pagar factura")]
    [EndpointDescription("Marca una factura como pagada registrando el monto abonado.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(InvoiceDetailDto))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> MarkAsPaid(
        Guid id,
        [FromBody] decimal amountPaid)
    {
        var result = await _service.MarkAsPaidAsync(id, amountPaid);
        return Ok(result);
    }

    // ============================================
    // CREAR NOTA DE CRÉDITO / DÉBITO
    // ============================================
    [HttpPost("{parentId:guid}/notes/{noteType}")]
    [EndpointName("CreateInvoiceNote")]
    [EndpointSummary("Crear nota de crédito/débito")]
    [EndpointDescription("Genera una nota de crédito o débito asociada a una factura padre específica.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(typeof(InvoiceDetailDto))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} ")]
    public async Task<IActionResult> CreateNote(
        Guid parentId,
        InvoiceType noteType,
        [FromBody] InvoiceCreateDto dto)
    {
        var result = await _service.CreateNoteAsync(parentId, dto, noteType);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = parentId }, result);
    }

    // ============================================
    // VERIFICAR SI TIENE NOTAS HIJAS
    // ============================================
    [HttpGet("{id:guid}/child-notes")]
    [EndpointName("GetInvoiceHasChildNotes")]
    [EndpointSummary("Verificar notas hijas")]
    [EndpointDescription("Comprueba y lista las notas de crédito o débito asociadas a una factura en particular.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(List<MinimalInvoiceDto>))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> HasChildNotes(Guid id)
    {
        var result = await _service.HasChildNotesAsync(id);
        return Ok(result);
    }

    // ============================================
    // RECALCULAR TOTALES
    // ============================================
    [HttpPost("{id:guid}/recalculate")]
    [EndpointName("CreateInvoiceByIdRecalculation")]
    [EndpointSummary("Recalcular totales de factura")]
    [EndpointDescription("Recalcula los totales y el estado derivado de una factura a partir de sus notas relacionadas.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> RecalculateTotals(Guid id)
    {
        await _service.RecalculateInvoiceTotalsAsync(id);
        return Ok();
    }

    // SUMMARY DE NOTAS (CREDIT/DEBIT)
    [HttpGet("{id:guid}/notes-summary")]
    [EndpointName("GetInvoiceRecalculateNotesSummary")]
    [EndpointSummary("Recalcular totales de factura")]
    [EndpointDescription("Vuelve a calcular los totales de una factura específica.")]
    [Tags("SIGREF - Facturación")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(InvoiceNotesSummaryDto))]
    [Authorize(Roles = $"{RolesConstants.cashier} ,  {RolesConstants.admin} , {RolesConstants.auditor}")]
    public async Task<IActionResult> GetNotesSummary(Guid id)
    {
        var result = await _service.GetNotesSummaryAsync(id);
        return Ok(result);
    }
}
