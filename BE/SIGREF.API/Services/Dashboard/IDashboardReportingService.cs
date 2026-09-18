using SIGREF.API.Dtos.Dashboard;

namespace SIGREF.API.Services.Dashboard;

public interface IDashboardReportingService
{
    /// <summary>
    ///     Resumen general del dashboard:
    ///     - Total ingresos
    ///     - Total servicios realizados
    ///     - Total pacientes atendidos
    ///     - Total de cierres de caja que requirieron corrección
    /// </summary>
    Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto filter);

    /// <summary>
    ///     Top 5 servicios más usados y bottom 5 menos usados.
    /// </summary>
    Task<ServiceUsageResultDto> GetServiceUsageAsync(DashboardFilterDto filter);

    /// <summary>
    ///     Top 5 paquetes más solicitados + agrupación de "Otros".
    /// </summary>
    Task<PackageUsageResultDto> GetPackageUsageAsync(DashboardFilterDto filter);

    /// <summary>
    ///     Ingresos distribuidos por semana dentro del rango seleccionado.
    /// </summary>
    Task<List<WeeklyIncomeDto>> GetWeeklyIncomeAsync(DashboardFilterDto filter);

    /// <summary>
    ///     Ingresos agrupados por turno (Shift) y por location.
    ///     Permite n turnos y n locations.
    /// </summary>
    Task<List<ShiftIncomeDto>> GetShiftIncomeAsync(DashboardFilterDto filter);

    /// <summary>
    ///     Ingresos totales agrupados por location.
    /// </summary>
    Task<List<LocationIncomeDto>> GetLocationIncomeAsync(DashboardFilterDto filter);
}