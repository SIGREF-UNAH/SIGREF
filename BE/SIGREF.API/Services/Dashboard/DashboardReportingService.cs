using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Dashboard;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.FhirUtils;

namespace SIGREF.API.Services.Dashboard;

/// <summary>
///  TODO :
/// REVISAR LAS FECHAS EXACTAS YA QUE NO SE COMO SE ESTA TRABAJANDO EN UTC AMERICA O EN ALGO MAS?
/// Revisar bien las consultas, supongo en cuando FE conecte saltaran errores o Diferencias
/// Traer los nombres de las locaciones
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
        // Linux / Docker
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Tegucigalpa");
        }
        catch
        {
            // Windows
            return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
    }

    private ResponseDto<(DateTime Start, DateTime End)> NormalizeDateRange(DashboardFilterDto filter)
    {
        var response = new ResponseDto<(DateTime Start, DateTime End)>();
        var tz = GetAppTimeZone();

        var todayLocal = DateTime.Today;

        // Helper: convierte "fecha local" -> (startUtc, endExclusiveUtc)
        (DateTime StartUtc, DateTime EndExclusiveUtc) ToUtcRange(DateTime startLocalDate, DateTime endLocalDate)
        {
            // Inicio de día local (Kind=Unspecified a propósito)
            var startLocal = DateTime.SpecifyKind(startLocalDate.Date, DateTimeKind.Unspecified);

            // Inicio del día siguiente local (exclusivo)
            var endExclusiveLocal = DateTime.SpecifyKind(endLocalDate.Date.AddDays(1), DateTimeKind.Unspecified);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
            var endExclusiveUtc = TimeZoneInfo.ConvertTimeToUtc(endExclusiveLocal, tz);

            return (startUtc, endExclusiveUtc);
        }

        // 1) No mandaron nada => este mes
        if (filter.StartDate == null && filter.EndDate == null)
        {
            var startLocal = new DateTime(todayLocal.Year, todayLocal.Month, 1);
            var endLocal = todayLocal;

            var (startUtc, endExclusiveUtc) = ToUtcRange(startLocal, endLocal);

            response.Status = true;
            response.Data = (startUtc, endExclusiveUtc);
            response.Message = "Rango aplicado: este mes (UTC).";
            return response;
        }

        // 2) Mandaron solo StartDate
        if (filter.StartDate != null && filter.EndDate == null)
        {
            var startLocal = filter.StartDate.Value.Date;

            if (startLocal > todayLocal)
            {
                response.Status = false;
                response.Message = "No se puede usar un StartDate futuro sin EndDate.";
                return response;
            }

            var endLocal = todayLocal;

            var (startUtc, endExclusiveUtc) = ToUtcRange(startLocal, endLocal);

            response.Status = true;
            response.Data = (startUtc, endExclusiveUtc);
            response.Message = "Rango aplicado: desde StartDate hasta hoy (UTC).";
            return response;
        }

        // 3) Mandaron solo EndDate
        if (filter.StartDate == null && filter.EndDate != null)
        {
            response.Status = false;
            response.Message = "StartDate es obligatorio si envías EndDate.";
            return response;
        }

        // 4) Mandaron ambos
        var startDateLocal = filter.StartDate!.Value.Date;
        var endDateLocal = filter.EndDate!.Value.Date;

        if (startDateLocal > endDateLocal)
        {
            response.Status = false;
            response.Message = "El rango de fechas es inválido.";
            return response;
        }

        var (startUtcFinal, endExclusiveUtcFinal) = ToUtcRange(startDateLocal, endDateLocal);

        response.Status = true;
        response.Data = (startUtcFinal, endExclusiveUtcFinal);
        response.Message = "Rango aplicado: StartDate a EndDate (UTC).";
        return response;
    }


    public async Task<ResponseDto<DashboardSummaryDto>> GetSummaryAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<DashboardSummaryDto>
        {
            Status = true,
            Data = new DashboardSummaryDto()
        };

        // ===============================
        // VALIDACIÓN / NORMALIZACIÓN FECHAS
        // ===============================
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startDate, endDate) = normalized.Data;

        // Para rangos por fecha con DateTime (incluye todo el EndDate)
        // Mejor práctica: [start, endExclusive)
        var start = startDate.Date;
        var endExclusive = endDate.Date.AddDays(1);

        // ===============================
        // BASE QUERY (Materialized View)
        // ===============================
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking()
            .AsQueryable();

        // --- Fecha ---
        facts = facts.Where(f =>
            f.CreatedDate >= start &&
            f.CreatedDate < endExclusive);

        // --- Locations ---
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

        // ===============================
        // RESULTADO
        // ===============================
        response.Data.TotalIncome = totalIncome;
        response.Data.TotalServices = totalServices;
        response.Data.TotalPatients = totalPatients;
        response.Data.TotalCashierClosuresWithErrors = closuresWithErrors;

        response.Message = "Resumen del dashboard generado correctamente.";
        return response;
    }


    public async Task<ResponseDto<ServiceUsageResultDto>> GetServiceUsageAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<ServiceUsageResultDto>
        {
            Status = true,
            Data = new ServiceUsageResultDto()
        };

        // ===============================
        // VALIDACIÓN / NORMALIZACIÓN FECHAS (UTC)
        // ===============================
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startUtc, endExclusiveUtc) = normalized.Data;

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


        // Si LocationIds vacío, manda null para que ({2} IS NULL...) sea true
        // OJO: esto asume que location_id es TEXT y filter.LocationIds es List<string>
        var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
            ? filter.LocationIds.ToArray()
            : null;

        var rows = await _dbContext
            .Set<ServiceUsageRow>()
            .FromSqlRaw(sql, startUtc, endExclusiveUtc, locationIdsArray)
            .AsNoTracking()
            .ToListAsync();

        if (rows.Count == 0)
        {
            response.Message = "No hay datos disponibles para este rango.";
            return response;
        }

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
        response.Data.TopUsed = top5.Select(s => new ServiceUsageDto
        {
            ServiceId = s.ServiceId,
            FhirServiceId = s.FhirServiceId ?? "",
            ServiceName = ResolveName(s.FhirServiceId, nameLookup),
            Count = s.Count,
            TotalGenerated = s.TotalGenerated
        }).ToList();

        response.Data.BottomUsed = bottom5.Select(s => new ServiceUsageDto
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
        var totalCount =
            response.Data.TopUsed.Sum(x => x.Count) +
            response.Data.BottomUsed.Sum(x => x.Count);

        if (totalCount > 0)
        {
            foreach (var s in response.Data.TopUsed)
                s.Percentage = Math.Round(((decimal)s.Count / totalCount) * 100m, 2);

            foreach (var s in response.Data.BottomUsed)
                s.Percentage = Math.Round(((decimal)s.Count / totalCount) * 100m, 2);
        }

        response.Message = "Uso de servicios obtenido correctamente.";
        return response;
    }


    public async Task<ResponseDto<PackageUsageResultDto>> GetPackageUsageAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<PackageUsageResultDto>
        {
            Status = true,
            Data = new PackageUsageResultDto()
        };

        // ===============================
        // VALIDACIÓN / NORMALIZACIÓN FECHAS (UTC)
        // ===============================
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startUtc, endExclusiveUtc) = normalized.Data;

        // ===============================
        // BASE QUERY: Materialized View (mv_dashboard_facts)
        // - Rango [startUtc, endExclusiveUtc)
        // - Solo filas con PackageId
        // ===============================
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking()
            .Where(f =>
                f.CreatedDate >= startUtc &&
                f.CreatedDate < endExclusiveUtc &&
                f.PackageId != null
            );

        // Filtro por Location
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
        // Si el MV repite real_income por cada item del invoice,
        // esto evita sumar y contar duplicado.
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
        // AGRUPAR PAQUETES (por invoices distintos)
        // ===============================
        var grouped = await packageInvoices
            .GroupBy(x => x.PackageId)
            .Select(g => new
            {
                PackageId = g.Key,
                Count = g.Count(), // cantidad de invoices con ese paquete
                TotalGenerated = g.Sum(x => x.RealIncome) ?? 0m
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        if (grouped.Count == 0)
        {
            response.Message = "No hay paquetes registrados en este rango.";
            return response;
        }

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
        response.Data.Top5 = top5.Select(p => new PackageUsageDto
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
        var totalCount = (decimal)response.Data.Top5.Sum(x => x.Count) + (others?.Count ?? 0);

        if (totalCount > 0)
        {
            foreach (var p in response.Data.Top5)
                p.Percentage = Math.Round(((decimal)p.Count / totalCount) * 100m, 2);

            if (others != null)
                others.Percentage = Math.Round(((decimal)others.Count / totalCount) * 100m, 2);
        }

        response.Data.Others = others;

        response.Message = "Ranking de paquetes generado correctamente.";
        return response;
    }


    public async Task<ResponseDto<List<WeeklyIncomeDto>>> GetWeeklyIncomeAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<List<WeeklyIncomeDto>>
        {
            Status = true,
            Data = new List<WeeklyIncomeDto>()
        };

        // 1) Normalizar rango [startUtc, endExclusiveUtc)
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startUtc, endExclusiveUtc) = normalized.Data;

        // Timezone (Honduras)
        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Tegucigalpa");

        // 2) Base query a la MV / tabla de facts
        // Ajustá el Set<DashboardFact>() a tu DbSet real si lo tenés tipado.
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking()
            .Where(f => f.CreatedDate >= startUtc && f.CreatedDate < endExclusiveUtc);

        // 3) Filtros opcionales
        if (filter.LocationIds?.Count > 0)
            facts = facts.Where(f => f.LocationId != null && filter.LocationIds.Contains(f.LocationId));

        //if (filter.ModuleIds?.Count > 0)
        //    facts = facts.Where(f => f.ModuleId != null && filter.ModuleIds.Contains(f.ModuleId.Value));

        if (filter.ShiftIds?.Count > 0)
            facts = facts.Where(f => f.ShiftId != null && filter.ShiftIds.Contains(f.ShiftId.Value));

        if (filter.ServiceId.HasValue)
            facts = facts.Where(f => f.ServiceId == filter.ServiceId.Value);

        // 4) Reducir a 1 fila por invoice (para no inflar por item_id)
        // Asumo que DashboardFact tiene: InvoiceId (Guid o long) y RealIncome (decimal)
        var invoiceFacts = facts
            .Select(f => new
            {
                f.InvoiceId,
                f.CreatedDate,
                RealIncome = (decimal?)f.RealIncome
            })
            .Distinct();

        // 5) Traer a memoria lo mínimo necesario para agrupar por semana local
        //    (EF no siempre traduce bien TimeZoneInfo a SQL)
        var rows = await invoiceFacts.ToListAsync();

        // 6) Agrupar por semana (lunes como inicio)
        //    - Convertimos CreatedDate (UTC) a local
        //    - Calculamos el lunes de esa semana
        var grouped = rows
            .Select(r =>
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(r.CreatedDate, DateTimeKind.Utc),
                    tz
                );

                var localDate = local.Date;

                // Monday-based week start
                int diff = (7 + (int)localDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var weekStart = localDate.AddDays(-diff);

                return new
                {
                    WeekStart = weekStart,
                    WeekEnd = weekStart.AddDays(6),
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

        response.Data = grouped;
        response.Message = grouped.Count == 0
            ? "No hay ingresos en el rango indicado."
            : "Ingresos semanales generados correctamente.";

        return response;
    }


    public async Task<ResponseDto<List<ShiftIncomeDto>>> GetShiftIncomeAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<List<ShiftIncomeDto>>
        {
            Status = true,
            Data = new List<ShiftIncomeDto>()
        };

        // ===============================
        // VALIDACIÓN / NORMALIZACIÓN FECHAS (UTC)
        // ===============================
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startUtc, endExclusiveUtc) = normalized.Data;

        // Si arrays vienen vacíos, mandamos null para que el SQL no filtre
        var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
            ? filter.LocationIds.ToArray()
            : null;

        var shiftIdsArray = (filter.ShiftIds != null && filter.ShiftIds.Count > 0)
            ? filter.ShiftIds.ToArray()
            : null;

        // ===============================
        // SQL: dedup por invoice para no inflar por item_id
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
            .FromSqlRaw(sql, startUtc, endExclusiveUtc, locationIdsArray, shiftIdsArray)
            .AsNoTracking()
            .ToListAsync();

        if (rows.Count == 0)
        {
            response.Message = "No hay datos disponibles para este rango.";
            return response;
        }

        response.Data = rows.Select(r => new ShiftIncomeDto
        {
            ShiftId = r.ShiftId,
            ShiftName = r.ShiftName ?? "",
            LocationId = r.LocationId ?? "",
            LocationName = r.LocationId ?? "", // NO existe location_name en la MV, por ahora usamos el id
            TotalInvoices = r.TotalInvoices,
            TotalIncome = r.TotalIncome
        }).ToList();

        response.Message = "Ingresos por turno y location generados correctamente.";
        return response;
    }

// Clase interna para mapear el resultado del SQL


    public async Task<ResponseDto<List<LocationIncomeDto>>> GetLocationIncomeAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<List<LocationIncomeDto>>
        {
            Status = true,
            Data = new List<LocationIncomeDto>()
        };

        // ===============================
        // VALIDACIÓN / NORMALIZACIÓN FECHAS (UTC)
        // ===============================
        var normalized = NormalizeDateRange(filter);
        if (!normalized.Status)
        {
            response.Status = false;
            response.Message = normalized.Message;
            return response;
        }

        var (startUtc, endExclusiveUtc) = normalized.Data;

        // Si LocationIds viene vacío, mandamos null para que el SQL NO filtre
        var locationIdsArray = (filter.LocationIds != null && filter.LocationIds.Count > 0)
            ? filter.LocationIds.ToArray()
            : null;

        // ===============================
        // SQL: dedup por invoice para no inflar por item_id
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
            .FromSqlRaw(sql, startUtc, endExclusiveUtc, locationIdsArray)
            .AsNoTracking()
            .ToListAsync();

        if (rows.Count == 0)
        {
            response.Message = "No hay datos disponibles para este rango.";
            return response;
        }

        response.Data = rows.Select(r => new LocationIncomeDto
        {
            LocationId = r.LocationId ?? "",
            LocationName = r.LocationId ?? "", // NO existe location_name en la MV
            TotalInvoices = r.TotalInvoices,
            TotalIncome = r.TotalIncome
        }).ToList();

        response.Message = "Ingresos por location generados correctamente.";
        return response;
    }
}