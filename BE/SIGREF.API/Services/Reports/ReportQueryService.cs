using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Billing;
using SIGREF.API.Database.Entity.common;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Report;
using SIGREF.API.Services.Auth;
using SIGREF.API.Services.Reports;

/// <summary>
/// Servicio de consulta de reportes usando InvoiceEntity directamente.
/// Esta versión es más confiable y no depende de la vista materializada DashboardFact.
/// </summary>
public class ReportQueryService : IReportQueryService
{
    private readonly SIGREFContext _context;
    private readonly IUserContextService _userContext;

    public ReportQueryService(
        SIGREFContext context,
        IUserContextService userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<ResponseDto<ReportSummaryResponseDto>> GetReportSummaryAsync(ReportFilterDto filter)
    {
        var error = ValidateFilter(filter);
        if (error != null)
            return new ResponseDto<ReportSummaryResponseDto>
            {
                Message = error,
                Status = false,
                StatusCode = 400,
                Data = null

            };

        var query = BaseQuery(filter);

        // Ejecutar queries en paralelo para mejor performance
        var summaryTask = query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalTransactions = g.LongCount(),

                TotalCollected = g
                    .Where(x => x.Status == InvoiceStatus.Paid)
                    .Select(x => (decimal?)x.FinalTotal)
                    .Sum() ?? 0m,

                PaidCount = g.Count(x => x.Status == InvoiceStatus.Paid),

                ExoneratedCount = g.Count(x => x.InvoiceType == InvoiceType.Exempt),

                CanceledCount = g.Count(x => x.Status == InvoiceStatus.Cancelled)
            })
            .FirstOrDefaultAsync();


        var seriesTask = query
            .Where(x => x.Serie != null)
            .Select(x => x.Serie!.Prefix + " - " + x.Serie.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        await Task.WhenAll(summaryTask, seriesTask);

        var summary = await summaryTask;
        var executedSeries = await seriesTask;

        return new ResponseDto<ReportSummaryResponseDto>
        {
            Status = true,
            StatusCode = 200, 
            Message = "OK",
            Data = new ReportSummaryResponseDto
            {
                ReportName = "Reporte de Ingresos",
                Summary = new ReportSummaryDto
                {
                    TotalTransactions = summary?.TotalTransactions ?? 0,
                    TotalCollected = summary?.TotalCollected ?? 0m,
                    PaidServicesCount = summary?.PaidCount ?? 0,
                    ExoneratedServicesCount = summary?.ExoneratedCount ?? 0,
                    CanceledServicesCount = summary?.CanceledCount ?? 0,
                    ExecutedSeries = executedSeries ?? new List<string>()
                },
                Metadata = BuildMetadata()
            }
        };

    }

    public async Task<ResponseDto<ReportDetailPageResponseDto>> GetReportDetailPageAsync(
        ReportFilterDto filter)
    {
        var error = ValidateFilter(filter);
        if (error != null)
            return new ResponseDto<ReportDetailPageResponseDto>
        {
            Status = false,
            StatusCode = 400,
            Message = error,
            Data = null
        };


        var pageNumber = Math.Max(filter.PageNumber, 1);
        var pageSize = Math.Clamp(filter.PageSize, 50, 500);

        var baseQuery = BaseQuery(filter);

        var totalItems = await baseQuery.LongCountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await baseQuery
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReportLineDto
            {
                TransactionDate = x.CreatedDate,
                ReceiptNumber = x.Serie!.Prefix + "-" + x.Number.ToString().PadLeft(8, '0'),
                
                // NOTA: CashierName necesita resolverse
                // TODO : RESOLVER EL CASHIER NAME
                
                CashierName = x.CashierSession != null 
                    ? "Usuario: " + x.CashierSession.UserId.ToString() 
                    : "Sin cajero",
                
                PatientName = x.PatientDisplay ?? "Sin paciente",
                
                // Manejar multiples servicios
                ServiceName = x.Items.Count == 1 
                    ? x.Items.First().Description 
                    : x.Items.Count > 1
                        ? $"Múltiples servicios ({x.Items.Count})"
                        : "Sin servicios",
                
                Status = x.Status.ToString(),
                AmountPaid = x.AmountPaid
            })
            .ToListAsync();

        return new ResponseDto<ReportDetailPageResponseDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "OK",
            Data = new ReportDetailPageResponseDto
            {
                ReportName = "Reporte de Ingresos",
                Metadata = BuildMetadata(),
                Pagination = new PaginationDto
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = pageNumber < totalPages,
                    HasPrevious = pageNumber > 1
                },
                Items = items
            }
        };

    }

    // =====================================================
    // Helpers
    // =====================================================

    private IQueryable<InvoiceEntity> BaseQuery(ReportFilterDto filter)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(x => x.Serie)
            .Include(x => x.CashierSession)
            .Include(x => x.Items)
            .Where(x =>
                x.CreatedDate >= filter.StartDate!.Value &&
                x.CreatedDate <= filter.EndDate!.Value);

        // Filtrar por SeriesIds si se especifica
        if (filter.SeriesIds != null && filter.SeriesIds.Any())
        {
            query = query.Where(x => filter.SeriesIds.Contains(x.SerieId));
        }

        // Filtrar por CashiersKeycloakIds si se especifica
        if (filter.CashiersKeycloakIds != null && filter.CashiersKeycloakIds.Any())
        {
            // Convertir los Keycloak IDs (strings) a Guids
            var cashierUserIds = filter.CashiersKeycloakIds
                .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            if (cashierUserIds.Any())
            {
                query = query.Where(x =>
                    x.CashierSession != null &&
                    cashierUserIds.Contains(x.CashierSession.UserId));
            }
        }

        return query;
    }

    private static string? ValidateFilter(ReportFilterDto filter)
    {
        if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            return "Debe especificar un rango de fechas válido.";

        if (filter.EndDate.Value < filter.StartDate.Value)
            return "La fecha final no puede ser menor que la inicial.";

        // Validar que el rango no sea excesivamente largo (prevenir queries pesadas)
        var dateRange = filter.EndDate.Value - filter.StartDate.Value;
        if (dateRange.TotalDays > 365)
            return "El rango de fechas no puede ser mayor a 1 año. Use la funcionalidad de exportación para rangos mayores.";

        return null;
    }

    private ReportMetadataDto BuildMetadata()
        => new()
        {
            GeneratedAt = DateTime.UtcNow,
            GeneratedByUserName = _userContext.GetUsername(),
            GeneratedByRoleName = _userContext.GetUserRoles()
        };
}
