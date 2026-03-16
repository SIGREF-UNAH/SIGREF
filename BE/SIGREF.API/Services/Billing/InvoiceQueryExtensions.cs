using SIGREF.API.Dtos.Invoice;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.API.Services.Billing;

public static class InvoiceQueryExtensions
{
    public static IQueryable<InvoiceEntity> ApplyInvoiceSorting(
        this IQueryable<InvoiceEntity> query,
        InvoiceSortField? sortBy,
        bool descending)
    {
        var desc = descending;
        var field = sortBy ?? InvoiceSortField.CreatedDate;

        return field switch
        {
            InvoiceSortField.Number =>
                desc ? query.OrderByDescending(i => i.Number)
                    : query.OrderBy(i => i.Number),

            InvoiceSortField.PatientDisplay =>
                desc ? query.OrderByDescending(i => i.PatientDisplay)
                    : query.OrderBy(i => i.PatientDisplay),

            InvoiceSortField.FinalTotal =>
                desc ? query.OrderByDescending(i => i.FinalTotal)
                    : query.OrderBy(i => i.FinalTotal),

            InvoiceSortField.Status =>
                desc ? query.OrderByDescending(i => i.Status)
                    : query.OrderBy(i => i.Status),

            InvoiceSortField.InvoiceType =>
                desc ? query.OrderByDescending(i => i.InvoiceType)
                    : query.OrderBy(i => i.InvoiceType),

            _ => // DEFAULT CreatedDate
                desc ? query.OrderByDescending(i => i.CreatedDate).ThenByDescending(i => i.Number)
                    : query.OrderBy(i => i.CreatedDate).ThenBy(i => i.Number)
        };
    }
}