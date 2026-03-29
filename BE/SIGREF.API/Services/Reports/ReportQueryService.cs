using Microsoft.EntityFrameworkCore;
using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Types;
using SIGREF.Core.Extensions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;

namespace SIGREF.API.Services.Reports;

/// <summary>
/// Servicio de consulta de reportes usando InvoiceEntity directamente.
/// Esta versión es más confiable y no depende de la vista materializada DashboardFact.
/// </summary>
public class ReportQueryService : IReportQueryService
{
    private readonly SIGREFContext _context;
    private readonly IUserContextService _userContext;
    private readonly IDbContextFactory<SIGREFContext> _dbFactory;

    public ReportQueryService(
        SIGREFContext context,
        IUserContextService userContext,
        IDbContextFactory<SIGREFContext> dbFactory)
    {
        _context = context;
        _userContext = userContext;
        _dbFactory = dbFactory;
    }
    
    //private IQueryable<InvoiceEntity> BaseQuery(ReportFilterDto filter)
    //    => BaseQuery(_context, filter);

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

        await using var db1 = await _dbFactory.CreateDbContextAsync();
        await using var db2 = await _dbFactory.CreateDbContextAsync();

        
        var query1 = db1.Invoices.ApplyBaseFilters(filter);
        var query2 = db2.Invoices.ApplyBaseFilters(filter);

        var summaryTask = query1
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

        var seriesTask = query2
            .Where(x => x.Serie != null)
            .Select(x => x.Serie!.Prefix + " - " + x.Serie.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        await Task.WhenAll(summaryTask, seriesTask);

        var summary = await summaryTask;
        var executedSeries = await seriesTask;

        var hospitalInfo = await _context.HospitalProperties
            .AsNoTracking()
            .Where(h => h.IsSingleton)
            .Select(h => new HospitalInfoDto
            {
                HospitalName = h.Name,
                DirectorName = h.Director ?? string.Empty,
                HospitalCode = h.HospitalCode ?? string.Empty,
                HospitalLogoImageId = h.LogoMediaId ?? Guid.Empty,
                UrlLogo = h.UrlLogo ?? string.Empty, 
                HealthDepartmentLogoImageId = h.HealthLogoMediaId ?? Guid.Empty,
                UrlLogoHealth = h.UrlLogoHealth ??  string.Empty,
                Contact = new HospitalContactDto
                {
                    PhoneNumber = h.PhoneNumber ?? string.Empty,
                    Email = h.Email ?? string.Empty,
                    Address = h.Location ?? string.Empty
                }
            })
            .FirstOrDefaultAsync();

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
                Metadata = BuildMetadata(),
                Hospital = hospitalInfo
            }
        };
    }


    public async Task<ResponseDto<ReportDetailPageResponseDto>> GetReportDetailPageAsync(ReportFilterDto filter)
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

      
        var baseQuery = _context.Invoices
            .ApplyBaseFilters(filter) 
            .AsNoTracking();

        // Total (solo filtros)
        var totalItems = await baseQuery.LongCountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Página
        var items = await baseQuery
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.CreatedDate,
                SeriePrefix = x.Serie != null ? x.Serie.Prefix : null,
                x.Number,
                CashierUserId = x.CashierSession != null ? (Guid?)x.CashierSession.UserId : null,
                x.PatientDisplay,
                ItemsCount = x.Items.Count(),
                FirstItemDesc = x.Items
                    .OrderBy(i => i.Id)
                    .Select(i => i.Description)
                    .FirstOrDefault(),
                Status = x.Status,
                x.AmountPaid
            })
            .Select(x => new ReportLineDto
            {
                TransactionDate = x.CreatedDate,
                ReceiptNumber = (x.SeriePrefix ?? "SIN") + "-" + x.Number.ToString().PadLeft(8, '0'),

                CashierName = x.CashierUserId.HasValue
                    ? "Usuario: " + x.CashierUserId.Value.ToString()
                    : "Sin cajero",

                PatientName = x.PatientDisplay ?? "Sin paciente",

                ServiceName = x.ItemsCount == 1
                    ? (x.FirstItemDesc ?? "Sin servicios")
                    : x.ItemsCount > 1
                        ? $"Múltiples servicios ({x.ItemsCount})"
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
    
    private static string? ValidateFilter(ReportFilterDto filter)
    {
        if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            return "Debe especificar un rango de fechas válido.";

        if (filter.EndDate.Value < filter.StartDate.Value)
            return "La fecha final no puede ser menor que la inicial.";

        // Validar que el rango no sea excesivamente largo (prevenir queries pesadas)
        var dateRange = filter.EndDate.Value - filter.StartDate.Value;
        if (dateRange.TotalDays > 365)
            return
                "El rango de fechas no puede ser mayor a 1 año. Use la funcionalidad de exportación para rangos mayores.";


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