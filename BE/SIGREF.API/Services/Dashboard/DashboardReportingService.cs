using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Dashboard;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.FhirUtils;

namespace SIGREF.API.Services.Dashboard;

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

    public async Task<ResponseDto<DashboardSummaryDto>> GetSummaryAsync(DashboardFilterDto filter)
    {
        var response = new ResponseDto<DashboardSummaryDto>
        {
            Status = true,
            Data = new DashboardSummaryDto()
        };

        // ===============================
        // VALIDACIÓN
        // ===============================
        if (filter.StartDate > filter.EndDate)
        {
            response.Status = false;
            response.Message = "El rango de fechas es inválido.";
            return response;
        }

        // ===============================
        // BASE QUERY (Materialized View)
        // ===============================
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking() // 
            .AsQueryable();

        // --- Fecha ---
        facts = facts.Where(f =>
            f.CreatedDate >= filter.StartDate &&
            f.CreatedDate <= filter.EndDate);

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
        var totalIncome = await facts.SumAsync(f => (decimal?)f.RealIncome) ?? 0;

        // ===============================
        // TOTAL SERVICIOS (total_items)
        // ===============================
        var totalServices = await facts.SumAsync(f => (int?)f.TotalItems) ?? 0;

        // ===============================
        // TOTAL PACIENTES (distinct)
        // ===============================
        var totalPatients = await facts
            .Where(f => f.PatientIdFhir != null)
            .Select(f => f.PatientIdFhir)
            .Distinct()
            .CountAsync();

        // ===============================
        // CIERRES DE CAJA CON ERROR
        // ===============================
        var cashierQuery = _dbContext.CashierSessions
            .AsNoTracking()
            .Include(c => c.Shift) // necesitado para LocationId
            .AsQueryable();

        cashierQuery = cashierQuery.Where(c =>
            c.ClosedAt != null &&
            c.ClosedAt >= filter.StartDate &&
            c.ClosedAt <= filter.EndDate);

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
        // VALIDACIÓN BÁSICA
        // ===============================
        if (filter.StartDate > filter.EndDate)
        {
            response.Status = false;
            response.Message = "El rango de fechas es inválido.";
            return response;
        }

        // ===============================
        // BASE QUERY: Materialized View
        // ===============================
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking()
            .Where(f =>
                f.CreatedDate >= filter.StartDate &&
                f.CreatedDate <= filter.EndDate &&
                f.ServiceId != null
            );

        // Filtro por location
        if (filter.LocationIds?.Count > 0)
        {
            facts = facts.Where(f =>
                f.LocationId != null &&
                filter.LocationIds.Contains(f.LocationId)
            );
        }

        // ===============================
        // TOP 5 SERVICIOS MÁS UTILIZADOS
        // ===============================
        var top5 = await facts
            .GroupBy(f => new { f.ServiceId, f.FhirServiceId })
            .Select(g => new
            {
                ServiceId = g.Key.ServiceId.Value,
                FhirServiceId = g.Key.FhirServiceId,
                Count = g.Count(),
                TotalGenerated = g.Sum(x => x.RealIncome)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // ===============================
        // BOTTOM 5 SERVICIOS MENOS UTILIZADOS
        // ===============================
        var bottom5 = await facts
            .GroupBy(f => new { f.ServiceId, f.FhirServiceId })
            .Select(g => new
            {
                ServiceId = g.Key.ServiceId.Value,
                FhirServiceId = g.Key.FhirServiceId,
                Count = g.Count(),
                TotalGenerated = g.Sum(x => x.RealIncome)
            })
            .OrderBy(x => x.Count)
            .Take(5)
            .ToListAsync();

        // ===============================
        // COMPROBAR SI HAY RESULTADOS
        // ===============================
        if (top5.Count == 0 && bottom5.Count == 0)
        {
            response.Message = "No hay datos disponibles para este rango.";
            return response;
        }

        // ===============================
        // ELIMINAR DUPLICADOS (Top tiene prioridad)
        // ===============================
        // -- Si se da la casualidad que tengamos 10 o menos servicios
        // -- Entonces estos tendran automaticamente al TOP 
        var topIds = top5.Select(x => x.ServiceId).ToHashSet();

        bottom5 = bottom5
            .Where(x => !topIds.Contains(x.ServiceId))
            .ToList();

        // ===============================
        // PREPARAR LISTA DE IDS FHIR (solo 10)
        // ===============================
        var fhirIds = top5
            .Select(x => x.FhirServiceId)
            .Concat(bottom5.Select(x => x.FhirServiceId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        // ===============================
        // LOOKUP DE NOMBRES DESDE FHIR (solo 10)
        // ===============================
        var nameLookup = await _fhirLookupService.GetServiceNamesAsync(fhirIds);
        

        // ===============================
        // MAPEO A DTO (TOP 5)
        // ===============================
        response.Data.TopUsed = top5.Select(s => new ServiceUsageDto
        {
            ServiceId = s.ServiceId,
            FhirServiceId = s.FhirServiceId,
            ServiceName = ResolveName(s.FhirServiceId, nameLookup),
            Count = s.Count,
            TotalGenerated = s.TotalGenerated
        }).ToList();

        // ===============================
        // MAPEO A DTO (BOTTOM 5)
        // ===============================
        response.Data.BottomUsed = bottom5.Select(s => new ServiceUsageDto
        {
            ServiceId = s.ServiceId,
            FhirServiceId = s.FhirServiceId,
            ServiceName =  ResolveName(s.FhirServiceId, nameLookup),
            Count = s.Count,
            TotalGenerated = s.TotalGenerated
        }).ToList();

        // ===============================
        // CALCULAR PORCENTAJES 
        // ===============================
        decimal totalCount = response.Data.TopUsed.Sum(x => x.Count)
                             + response.Data.BottomUsed.Sum(x => x.Count);
        // ========================
        //  Calculo de Percentajes
        // ========================
        if (totalCount > 0)
        {
            foreach (var s in response.Data.TopUsed)
                s.Percentage = Math.Round((s.Count / totalCount) * 100m, 2);

            foreach (var s in response.Data.BottomUsed)
                s.Percentage = Math.Round((s.Count / totalCount) * 100m, 2);
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
        // VALIDACIÓN DE FECHAS
        // ===============================
        if (filter.StartDate > filter.EndDate)
        {
            response.Status = false;
            response.Message = "El rango de fechas es inválido.";
            return response;
        }

        // ===============================
        // BASE QUERY: Materialized View
        // ===============================
        var facts = _dbContext.Set<DashboardFact>()
            .AsNoTracking()
            .Where(f =>
                f.CreatedDate >= filter.StartDate &&
                f.CreatedDate <= filter.EndDate &&
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
        // AGRUPAR PAQUETES
        // ===============================
        var grouped = await facts
            .GroupBy(f => f.PackageId!)
            .Select(g => new
            {
                PackageId = g.Key,
                Count = g.Count(),
                TotalGenerated = g.Sum(x => x.RealIncome)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        if (grouped.Count == 0)
        {
            response.Message = "No hay paquetes registrados en este rango.";
            return response;
        }

        // ===============================
        // TOP 5
        // ===============================
        var top5 = grouped.Take(5).ToList();

        // ===============================
        // OTROS (resto combinado)
        // ===============================
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
                Percentage = 0 // se calcula luego
            };
        }

        // ===============================
        // OBTENER NOMBRES DESDE FHIR
        // TOP 5 + 1 "Otros" = máx 6 IDs
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
            PackageName =  ResolveName(p.PackageId, nameLookup),
            Count = p.Count,
            TotalGenerated = p.TotalGenerated
        }).ToList();

        // ===============================
        // CÁLCULO DE PORCENTAJES
        // ===============================
        decimal totalCount = response.Data.Top5.Sum(x => x.Count)
                             + (others?.Count ?? 0);

        if (totalCount > 0)
        {
            foreach (var p in response.Data.Top5)
                p.Percentage = Math.Round((p.Count / totalCount) * 100m, 2);

            if (others != null)
                others.Percentage = Math.Round((others.Count / totalCount) * 100m, 2);
        }

        response.Data.Others = others;

        response.Message = "Raking de paquetes generado correctamente";
        return response;
    }


    public Task<ResponseDto<List<WeeklyIncomeDto>>> GetWeeklyIncomeAsync(DashboardFilterDto filter)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto<List<ShiftIncomeDto>>> GetShiftIncomeAsync(DashboardFilterDto filter)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto<List<LocationIncomeDto>>> GetLocationIncomeAsync(DashboardFilterDto filter)
    {
        throw new NotImplementedException();
    }
}