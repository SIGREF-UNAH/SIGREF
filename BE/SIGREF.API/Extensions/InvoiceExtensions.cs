using SIGREF.API.Dtos.Invoice;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.API.Extensions;

public static class InvoiceExtensions
{
    /// <summary>
    /// Construye el InvoiceDetailDto a partir de una entidad cargada con sus ítems.
    /// Centraliza el mapeo para no duplicarlo en cada método.
    /// </summary>
    public static InvoiceDetailDto MapToDetail(InvoiceEntity invoice) => new()
    {
        Id               = invoice.Id,
        PatientIdFhir    = invoice.PatientIdFhir,
        PatientDisplay   = invoice.PatientDisplay,
 
        TotalOriginal    = invoice.TotalOriginal,
        InvoiceDiscount  = invoice.InvoiceDiscount,
        AdjustmentTotal  = invoice.AdjustmentTotal,
        FinalTotal       = invoice.FinalTotal,
        AmountPaid       = invoice.AmountPaid,
        AmountDue        = invoice.AmountDue,
 
        Status           = invoice.Status,
        InvoiceType      = invoice.InvoiceType,
        PaymentMethod    = invoice.PaymentMethod,
 
        SerieId          = invoice.SerieId,
        Number           = invoice.Number,
        CreatedDate      = invoice.CreatedDate,
        CreatedById      = invoice.CreatedById,
        ParentInvoiceId  = invoice.ParentInvoiceId,
 
        Items = invoice.Items.Select(x => new InvoiceItemDetailDto
        {
            Id          = x.Id,
            Description = x.Description,
            Quantity    = x.Quantity,
            UnitPrice   = x.UnitPrice,
            TotalAmount = x.TotalAmount
        }).ToList()
    };
}