using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Middleware;
using SIGREF.API.Services.FhirUtils;
using SIGREF.Common.Exceptions;
using SIGREF.Core.Entity.Dashboard;
using SIGREF.Infrastructure.Persistence;
using Hl7.Fhir.Rest;

namespace SIGREF.API.Services.Dashboard;

/// <summary>
/// TODO:
/// - Revisar bien las consultas; cuando FE conecte pueden saltar errores o diferencias.
/// - Traer los nombres de las locaciones desde FHIR o lookup local.
/// </summary>
public class DashboardReportingService : IDashboardReportingService
{
    private readonly SIGREFContext _dbContext;
    private readonly IFhirLookupService _fhirLookupService;

    public DashboardReportingService(SIGREFContext dbContext, IFhirLookupService fhirLookupService)
    {
        _dbContext = dbContext;
        _fhirLookupService = fhirLookupService;
    }

    private string ResolveName(string id, Dictionary<string, string?> lookup)
    {
        if (lookup.TryGetValue(id, out var name))
            return name ?? "N/A";

        return "N/A";
    }

    private static TimeZoneInfo GetAppTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Tegucigalpa");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
    }

    private static readonly TimeZoneInfo AppTimeZone = GetAppTimeZone();

    private (DateTimeOffset Start, DateTimeOffset End) NormalizeDateRange(DashboardFilterDto filter)
    {
        var todayLocal = DateTimeOffset.Now.Date;

        (DateTimeOffset Start, DateTimeOffset EndExclusive) ToOffsetRange(DateTimeOffset startLocalDate, DateTimeOffset endLocalDate)
        {
            var start = new DateTimeOffset(startLocalDate.Date, AppTimeZone.GetUtcOffset(startLocalDate.Date));
            var endExclusive = new DateTimeOffset(endLocalDate.Date.AddDays(1), AppTimeZone.GetUtcOffset(endLocalDate.Date.AddDays(1)));

            // PostgreSQL timestamptz/Npgsql requiere parámetros DateTimeOffset con offset UTC.
            return (start.ToUniversalTime(), endExclusive.ToUniversalTime());
        }

        // 1) No mandaron nada => este mes
        if (filter.StartDate == null && filter.EndDate == null)
        {
            var startLocal = new DateTimeOffset(new DateTime(todayLocal.Year, todayLocal.Month, 1), AppTimeZone.GetUtcOffset(new DateTime(todayLocal.Year, todayLocal.Month, 1)));
            var endLocal = new DateTimeOffset(todayLocal, AppTimeZone.GetUtcOffset(todayLocal));

            return ToOffsetRange(startLocal, endLocal);
        }

        // 2) Mandaron solo StartDate
        if (filter.StartDate != null && filter.EndDate == null)
        {
            var startLocal = new DateTimeOffset(filter.StartDate.Value.Date, AppTimeZone.GetUtcOffset(filter.StartDate.Value.Date));

            if (startLocal.Date > todayLocal)
                throw new ValidationException("DASHBOARD_FUTURE_START_DATE", new Dictionary<string, object>
                {
                    { "StartDate", startLocal.Date },
                    { "Today", todayLocal }
                });

            var endLocal = new DateTimeOffset(todayLocal, AppTimeZone.GetUtcOffset(todayLocal));
            return ToOffsetRange(startLocal, endLocal);
        }

        // 3) Mandaron solo EndDate
        if (filter.StartDate == null && filter.EndDate != null)
        {
            throw new ValidationException("DASHBOARD_START_DATE_REQUIRED", new Dictionary<string, object>
            {
                { "EndDate", filter.EndDate.Value }
            });
        }

        // 4) Mandaron ambos
        var startDateLocal = new DateTimeOffset(filter.StartDate!.Value.Date, AppTimeZone.GetUtcOffset(filter.StartDate.Value.Date));
        var endDateLocal = new DateTimeOffset(filter.EndDate!.Value.Date, AppTimeZone.GetUtcOffset(filter.EndDate.Value.Date));

        if (startDateLocal.Date > endDateLocal.Date)
            throw new ValidationException("DASHBOARD_INVALID_DATE_RANGE", new Dictionary<string, object>
            {
                { "StartDate", startDateLocal.Date },
                { "EndDate", endDateLocal.Date }
            });

        return ToOffsetRange(startDateLocal, endDateLocal);
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            // ===============================
            // BASE QUERY (Materialized View)
            // ===============================
            var facts = _dbContext.Set<DashboardFact>()
                .AsNoTracking()
                .AsQueryable();

            facts = facts.Where(f =>
                f.CreatedDate >= start &&
                f.CreatedDate < endExclusive);

            if (filter.LocationIds?.Count > 0)
            {
                facts = facts.Where(f =>
                    f.LocationId != null &&
                    filter.LocationIds.Contains(f.LocationId));
            }

            // ===============================
            // TOTAL INGRESOS (real_income)
            // ===============================
            var totalIncome = await facts.SumAsync(f => (decimal?)f.RealIncome) ?? 0m;

            // ===============================
            // TOTAL SERVICIOS (total_items)
            // ===============================
            var totalServices = await facts.SumAsync(f => (int?)f.TotalItems) ?? 0;

            // ===============================
            // TOTAL PACIENTES (distinct)
            // ===============================
            var totalPatients = await facts
                .Where(f => f.PatientIdFhir != null)
                .Select(f => f.PatientIdFhir!)
                .Distinct()
                .CountAsync();

            // ===============================
            // CIERRES DE CAJA CON ERROR
            // ===============================
            var cashierQuery = _dbContext.CashierSessions
                .AsNoTracking()
                .Include(c => c.Shift)
                .AsQueryable();

            cashierQuery = cashierQuery.Where(c =>
                c.ClosedAt != null &&
                c.ClosedAt >= start &&
                c.ClosedAt < endExclusive);

            if (filter.LocationIds?.Count > 0)
            {
                cashierQuery = cashierQuery.Where(c =>
                    c.Shift != null &&
                    filter.LocationIds.Contains(c.Shift.LocationId));
            }

            var closuresWithErrors = await cashierQuery
                .CountAsync(c => !c.IsOpen && c.RequiresCorrection);

            return new DashboardSummaryDto
            {
                TotalIncome = totalIncome,
                TotalServices = totalServices,
                TotalPatients = totalPatients,
                TotalCashierClosuresWithErrors = closuresWithErrors
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "DashboardSummary", nameof(GetSummaryAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_DASHBOARD_SUMMARY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetSummaryAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetSummaryAsync) }
            });
        }
    }

    public async Task<ServiceUsageResultDto> GetServiceUsageAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            // ===============================
            // CONSULTA SQL OPTIMIZADA (SIN BETWEEN)
            // ===============================
            var sql = @"
                WITH svc AS (
                    SELECT
                        service_id AS ""ServiceId"",
                        health_service_id_fhir AS ""FhirServiceId"",
                        COUNT(*) AS ""Count"",
                        COALESCE(SUM(real_income), 0) AS ""TotalGenerated"",
                        ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS rn_desc,
                        ROW_NUMBER() OVER (ORDER BY COUNT(*) ASC)  AS rn_asc
                    FROM mv_dashboard_facts
                    WHERE created_date >= {0} AND created_date < {1}
                      AND service_id IS NOT NULL
                      AND (array_length({2}::text[], 1) IS NULL OR location_id = ANY({2}::text[]))
                    GROUP BY service_id, health_service_id_fhir
                )
                SELECT ""ServiceId"", ""FhirServiceId"", ""Count"", ""TotalGenerated""
                FROM svc
                WHERE rn_desc <= 5 OR rn_asc <= 5;
                ";

            var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
                ? filter.LocationIds.ToArray()
                : null;

            var rows = await _dbContext
                .Set<ServiceUsageRow>()
                .FromSqlRaw(sql, start, endExclusive, locationIdsArray)
                .AsNoTracking()
                .ToListAsync();

            if (rows.Count == 0)
                return new ServiceUsageResultDto
                {
                    TopUsed = new List<ServiceUsageDto>(),
                    BottomUsed = new List<ServiceUsageDto>()
                };

            // ===============================
            // SEPARAR TOP Y BOTTOM (máx. 10)
            // ===============================
            var top5 = rows.OrderByDescending(x => x.Count).Take(5).ToList();
            var bottom5 = rows.OrderBy(x => x.Count).Take(5).ToList();

            // ===============================
            // FHIR LOOKUP (cacheado)
            // ===============================
            var fhirIds = rows
                .Select(x => x.FhirServiceId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var nameLookup = await _fhirLookupService.GetServiceNamesAsync(fhirIds);

            // ===============================
            // MAPEO A DTOs
            // ===============================
            var topUsed = top5.Select(s => new ServiceUsageDto
            {
                ServiceId = s.ServiceId,
                FhirServiceId = s.FhirServiceId ?? "",
                ServiceName = ResolveName(s.FhirServiceId, nameLookup),
                Count = s.Count,
                TotalGenerated = s.TotalGenerated
            }).ToList();

            var bottomUsed = bottom5.Select(s => new ServiceUsageDto
            {
                ServiceId = s.ServiceId,
                FhirServiceId = s.FhirServiceId ?? "",
                ServiceName = ResolveName(s.FhirServiceId, nameLookup),
                Count = s.Count,
                TotalGenerated = s.TotalGenerated
            }).ToList();

            // ===============================
            // PORCENTAJES
            // ===============================
            var totalCount = topUsed.Sum(x => x.Count) + bottomUsed.Sum(x => x.Count);

            if (totalCount > 0)
            {
                foreach (var s in topUsed)
                    s.Percentage = Math.Round(((decimal)s.Count / totalCount) * 100m, 2);

                foreach (var s in bottomUsed)
                    s.Percentage = Math.Round(((decimal)s.Count / totalCount) * 100m, 2);
            }

            return new ServiceUsageResultDto
            {
                TopUsed = topUsed,
                BottomUsed = bottomUsed
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "ServiceUsage", nameof(GetServiceUsageAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_SERVICE_USAGE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetServiceUsageAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetServiceUsageAsync) }
            });
        }
    }

    public async Task<PackageUsageResultDto> GetPackageUsageAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            // ===============================
            // BASE QUERY: Materialized View
            // ===============================
            var facts = _dbContext.Set<DashboardFact>()
                .AsNoTracking()
                .Where(f =>
                    f.CreatedDate >= start &&
                    f.CreatedDate < endExclusive &&
                    f.PackageId != null
                );

            if (filter.LocationIds?.Count > 0)
            {
                facts = facts.Where(f =>
                    f.LocationId != null &&
                    filter.LocationIds.Contains(f.LocationId)
                );
            }

            // ===============================
            // EVITAR INFLAR CON item_id:
            // Nos quedamos con "1 fila por invoice" (por paquete).
            // ===============================
            var packageInvoices = facts
                .Select(f => new
                {
                    PackageId = f.PackageId!,
                    f.InvoiceId,
                    RealIncome = (decimal?)f.RealIncome
                })
                .Distinct();

            // ===============================
            // AGRUPAR PAQUETES
            // ===============================
            var grouped = await packageInvoices
                .GroupBy(x => x.PackageId)
                .Select(g => new
                {
                    PackageId = g.Key,
                    Count = g.Count(),
                    TotalGenerated = g.Sum(x => x.RealIncome) ?? 0m
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            if (grouped.Count == 0)
                return new PackageUsageResultDto
                {
                    Top5 = new List<PackageUsageDto>(),
                    Others = null
                };

            // ===============================
            // TOP 5 + OTROS
            // ===============================
            var top5 = grouped.Take(5).ToList();
            var othersSource = grouped.Skip(5).ToList();

            PackageUsageDto? others = null;
            if (othersSource.Count > 0)
            {
                others = new PackageUsageDto
                {
                    FhirPackageId = "others",
                    PackageName = "Otros",
                    Count = othersSource.Sum(x => x.Count),
                    TotalGenerated = othersSource.Sum(x => x.TotalGenerated),
                    Percentage = 0m
                };
            }

            // ===============================
            // OBTENER NOMBRES DESDE FHIR (solo TOP 5)
            // ===============================
            var fhirIds = top5
                .Select(x => x.PackageId)
                .Distinct()
                .ToList();

            var nameLookup = await _fhirLookupService.GetPackageNamesAsync(fhirIds);

            // ===============================
            // MAPEAR TOP 5
            // ===============================
            var top5Dtos = top5.Select(p => new PackageUsageDto
            {
                FhirPackageId = p.PackageId,
                PackageName = ResolveName(p.PackageId, nameLookup),
                Count = p.Count,
                TotalGenerated = p.TotalGenerated,
                Percentage = 0m
            }).ToList();

            // ===============================
            // PORCENTAJES
            // ===============================
            var totalCount = (decimal)top5Dtos.Sum(x => x.Count) + (others?.Count ?? 0);

            if (totalCount > 0)
            {
                foreach (var p in top5Dtos)
                    p.Percentage = Math.Round(((decimal)p.Count / totalCount) * 100m, 2);

                if (others != null)
                    others.Percentage = Math.Round(((decimal)others.Count / totalCount) * 100m, 2);
            }

            return new PackageUsageResultDto
            {
                Top5 = top5Dtos,
                Others = others
            };
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "PackageUsage", nameof(GetPackageUsageAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_PACKAGE_USAGE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetPackageUsageAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetPackageUsageAsync) }
            });
        }
    }

    public async Task<List<WeeklyIncomeDto>> GetWeeklyIncomeAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            // ===============================
            // BASE QUERY
            // ===============================
            var facts = _dbContext.Set<DashboardFact>()
                .AsNoTracking()
                .Where(f => f.CreatedDate >= start && f.CreatedDate < endExclusive);

            if (filter.LocationIds?.Count > 0)
                facts = facts.Where(f => f.LocationId != null && filter.LocationIds.Contains(f.LocationId));

            if (filter.ShiftIds?.Count > 0)
                facts = facts.Where(f => f.ShiftId != null && filter.ShiftIds.Contains(f.ShiftId.Value));

            if (filter.ServiceId.HasValue)
            {
                var serviceId = filter.ServiceId.Value;
                facts = facts.Where(f => f.ServiceId == serviceId);
            }

            // ===============================
            // DEDUP POR INVOICE
            // ===============================
            var invoiceFacts = facts
                .Select(f => new
                {
                    f.InvoiceId,
                    f.CreatedDate,
                    RealIncome = (decimal?)f.RealIncome
                })
                .Distinct();

            // Traer a memoria para agrupar por semana local
            var rows = await invoiceFacts.ToListAsync();

            // ===============================
            // AGRUPAR POR SEMANA (lunes como inicio)
            // ===============================
            var grouped = rows
                .Select(r =>
                {
                    var localDate = r.CreatedDate.ToOffset(AppTimeZone.GetUtcOffset(r.CreatedDate)).Date;
                    int diff = (7 + (int)localDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    var weekStart = localDate.AddDays(-diff);

                    return new
                    {
                        WeekStart = weekStart,
                        r.InvoiceId,
                        Income = r.RealIncome ?? 0m
                    };
                })
                .GroupBy(x => x.WeekStart)
                .OrderBy(g => g.Key)
                .Select(g => new WeeklyIncomeDto
                {
                    WeekStart = g.Key,
                    WeekEnd = g.Key.AddDays(6),
                    TotalIncome = g.Sum(x => x.Income),
                    InvoiceCount = g.Count()
                })
                .ToList();

            return grouped;
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "WeeklyIncome", nameof(GetWeeklyIncomeAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_WEEKLY_INCOME_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetWeeklyIncomeAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetWeeklyIncomeAsync) }
            });
        }
    }

    public async Task<List<ShiftIncomeDto>> GetShiftIncomeAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
                ? filter.LocationIds.ToArray()
                : null;

            var shiftIdsArray = (filter.ShiftIds != null && filter.ShiftIds.Count > 0)
                ? filter.ShiftIds.ToArray()
                : null;

            // ===============================
            // SQL: dedup por invoice
            // ===============================
            var sql = @"
                    WITH inv AS (
                        SELECT DISTINCT
                            invoice_id,
                            shift_id,
                            shift_name,
                            location_id,
                            real_income
                        FROM mv_dashboard_facts
                        WHERE created_date >= {0} AND created_date < {1}
                          AND invoice_id IS NOT NULL
                          AND shift_id IS NOT NULL
                          AND location_id IS NOT NULL
                          AND (array_length({2}::text[], 1) IS NULL OR location_id = ANY({2}::text[]))
                          AND (array_length({3}::uuid[], 1) IS NULL OR shift_id = ANY({3}::uuid[]))
                    )
                    SELECT
                        shift_id      AS ""ShiftId"",
                        shift_name    AS ""ShiftName"",
                        location_id   AS ""LocationId"",
                        COUNT(*) AS ""TotalInvoices"",
                        COALESCE(SUM(real_income), 0) AS ""TotalIncome""
                    FROM inv
                    GROUP BY shift_id, shift_name, location_id
                    ORDER BY ""TotalIncome"" DESC, ""TotalInvoices"" DESC;
                ";

            var rows = await _dbContext
                .Set<ShiftIncomeRow>()
                .FromSqlRaw(sql, start, endExclusive, locationIdsArray, shiftIdsArray)
                .AsNoTracking()
                .ToListAsync();

            return rows.Select(r => new ShiftIncomeDto
            {
                ShiftId = r.ShiftId,
                ShiftName = r.ShiftName ?? "",
                LocationId = r.LocationId ?? "",
                LocationName = r.LocationId ?? "", // TODO: reemplazar con lookup real
                TotalInvoices = r.TotalInvoices,
                TotalIncome = r.TotalIncome
            }).ToList();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "ShiftIncome", nameof(GetShiftIncomeAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_SHIFT_INCOME_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetShiftIncomeAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetShiftIncomeAsync) }
            });
        }
    }

    public async Task<List<LocationIncomeDto>> GetLocationIncomeAsync(DashboardFilterDto filter)
    {
        try
        {
            var (start, endExclusive) = NormalizeDateRange(filter);

            var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
                ? filter.LocationIds.ToArray()
                : null;

            // ===============================
            // SQL: dedup por invoice
            // ===============================
            var sql = @"
                    WITH inv AS (
                        SELECT DISTINCT
                            invoice_id,
                            location_id,
                            real_income
                        FROM mv_dashboard_facts
                        WHERE created_date >= {0} AND created_date < {1}
                          AND invoice_id IS NOT NULL
                          AND location_id IS NOT NULL
                          AND (array_length({2}::text[], 1) IS NULL OR location_id = ANY({2}::text[]))
                    )
                    SELECT
                        location_id   AS ""LocationId"",
                        COUNT(*) AS ""TotalInvoices"",
                        COALESCE(SUM(real_income), 0) AS ""TotalIncome""
                    FROM inv
                    GROUP BY location_id
                    ORDER BY ""TotalIncome"" DESC, ""TotalInvoices"" DESC;
                ";

            var rows = await _dbContext
                .Set<LocationIncomeRow>()
                .FromSqlRaw(sql, start, endExclusive, locationIdsArray)
                .AsNoTracking()
                .ToListAsync();

            return rows.Select(r => new LocationIncomeDto
            {
                LocationId = r.LocationId ?? "",
                LocationName = r.LocationId ?? "", // TODO: reemplazar con lookup real
                TotalInvoices = r.TotalInvoices,
                TotalIncome = r.TotalIncome
            }).ToList();
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "LocationIncome", nameof(GetLocationIncomeAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_LOCATION_INCOME_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetLocationIncomeAsync) }
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
            throw new ExternalServiceException("INTERNAL_DASHBOARD_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetLocationIncomeAsync) }
            });
        }
    }
}
