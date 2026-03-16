using SIGREF.Common.Dtos.Reports;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.Core.Extensions;

public static class InvoiceQueryExtensions
{
    public static IQueryable<InvoiceEntity> ApplyBaseFilters(
        this IQueryable<InvoiceEntity> query,
        ReportFilterDto filter)
    {
        // ══════════════════════════════════════════════════════
        // 1. Normalización de fechas a UTC
        // ══════════════════════════════════════════════════════
        var start = filter.StartDate!.Value;
        var end   = filter.EndDate!.Value;

        if (start.Kind == DateTimeKind.Unspecified)
            start = DateTime.SpecifyKind(start, DateTimeKind.Utc);

        if (end.Kind == DateTimeKind.Unspecified)
            end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        // ══════════════════════════════════════════════════════
        // 2. Filtro de rango de fechas
        // ══════════════════════════════════════════════════════
        query = query.Where(x => x.CreatedDate >= start && x.CreatedDate <= end);

        // ══════════════════════════════════════════════════════
        // 3. Filtro por Series (opcional)
        // ══════════════════════════════════════════════════════
        if (filter.SeriesIds is { Count: > 0 })
        {
            query = query.Where(x => filter.SeriesIds.Contains(x.SerieId));
        }

        // ══════════════════════════════════════════════════════
        // 4. Filtro por Cajeros (opcional)
        //
        // Convertimos los strings a Guid en memoria antes de
        // armar el predicado SQL para que EF pueda traducirlo
        // correctamente a un IN(...) parametrizado.
        // ══════════════════════════════════════════════════════
        if (filter.CashiersKeycloakIds is { Count: > 0 })
        {
            // Guid.TryParse descarta silenciosamente IDs malformados
            var cashierGuids = filter.CashiersKeycloakIds
                .Select(id => Guid.TryParse(id, out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            if (cashierGuids.Count > 0)
            {
                query = query.Where(x =>
                    x.CashierSession != null &&
                    cashierGuids.Contains(x.CashierSession.UserId));
            }
        }

        return query;
    }
}