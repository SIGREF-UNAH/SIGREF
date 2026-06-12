using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Middleware;
using SIGREF.Common.Dtos;
using SIGREF.Common.Dtos.Report;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Types;
using SIGREF.Core.Extensions;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using DateTime = System.DateTime;

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

    public async Task<ReportSummaryResponseDto> GetReportSummaryAsync(ReportFilterDto filter)
    {
        ValidateFilter(filter);

        await using var db1 = await _dbFactory.CreateDbContextAsync();
        await using var db2 = await _dbFactory.CreateDbContextAsync();

        try
        {
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
                    UrlLogoHealth = h.UrlLogoHealth ?? string.Empty,
                    Contact = new HospitalContactDto
                    {
                        PhoneNumber = h.PhoneNumber ?? string.Empty,
                        Email = h.Email ?? string.Empty,
                        Address = h.Location ?? string.Empty
                    }
                })
                .FirstOrDefaultAsync();

            return new ReportSummaryResponseDto
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
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "ReportSummary", "GetReportSummaryAsync");
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_QUERY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetReportSummaryAsync) }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_REPORT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetReportSummaryAsync) }
            });
        }
    }

    public async Task<ReportDetailPageResponseDto> GetReportDetailPageAsync(ReportFilterDto filter)
    {
        ValidateFilter(filter);

        var pageNumber = Math.Max(filter.PageNumber, 1);
        var pageSize = Math.Clamp(filter.PageSize, 50, 500);

        try
        {
            var baseQuery = _context.Invoices
                .ApplyBaseFilters(filter)
                .AsNoTracking();

            var totalItems = await baseQuery.LongCountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

            var items = await baseQuery
                .OrderByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReportLineProjection
                {
                    CreatedDate = x.CreatedDate,
                    SeriePrefix = x.Serie != null ? x.Serie.Prefix : null,
                    Number = x.Number,
                    CashierUserId = x.CashierSession != null ? (Guid?)x.CashierSession.UserId : null,
                    PatientDisplay = x.PatientDisplay,
                    ItemsCount = x.Items.Count(),
                    FirstItemDesc = x.Items
                        .OrderBy(i => i.Id)
                        .Select(i => i.Description)
                        .FirstOrDefault(),
                    Status = x.Status,
                    AmountPaid = x.AmountPaid
                })
                .ToListAsync();

            var reportLines = items.Select(x => new ReportLineDto
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
            }).ToList();

            return new ReportDetailPageResponseDto
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
                Items = reportLines
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "ReportDetailPage", "GetReportDetailPageAsync");
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_QUERY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetReportDetailPageAsync) }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_REPORT_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetReportDetailPageAsync) }
            });
        }
    }

    private static void ValidateFilter(ReportFilterDto filter)
    {
        if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            throw new ValidationException("REPORT_MISSING_DATE_RANGE");

        if (filter.EndDate.Value < filter.StartDate.Value)
            throw new ValidationException("REPORT_INVALID_DATE_RANGE", new Dictionary<string, object>
            {
                { "StartDate", filter.StartDate.Value },
                { "EndDate", filter.EndDate.Value }
            });

        var dateRange = filter.EndDate.Value - filter.StartDate.Value;
        if (dateRange.TotalDays > 365)
            throw new ValidationException("REPORT_DATE_RANGE_TOO_LARGE", new Dictionary<string, object>
            {
                { "MaxDaysAllowed", 365 },
                { "RequestedDays", (int)dateRange.TotalDays }
            });
    }

    private ReportMetadataDto BuildMetadata()
        => new()
        {
            GeneratedAt = DateTime.UtcNow,
            GeneratedByUserName = _userContext.GetUsername(),
            GeneratedByRoleName = _userContext.GetUserRoles()
        };

    /// <summary>
    /// Proyección intermedia para evitar proyecciones anidadas complejas en EF Core.
    /// </summary>
    private sealed class ReportLineProjection
    {
        public DateTimeOffset CreatedDate { get; set; }
        public string? SeriePrefix { get; set; }
        public long Number { get; set; }
        public Guid? CashierUserId { get; set; }
        public string? PatientDisplay { get; set; }
        public int ItemsCount { get; set; }
        public string? FirstItemDesc { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal AmountPaid { get; set; }
    }
}