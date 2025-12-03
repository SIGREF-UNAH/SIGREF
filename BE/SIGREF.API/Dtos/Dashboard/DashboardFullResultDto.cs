namespace SIGREF.API.Dtos.Dashboard;

public class DashboardFullResultDto
{
    /// <summary>
    /// Resumen general del dashboard.
    /// (ingresos, servicios, pacientes, cierres con errores)
    /// </summary>
    public DashboardSummaryDto Summary { get; set; } = new();

    /// <summary>
    /// Servicios más usados (top/bottom).
    /// </summary>
    public ServiceUsageResultDto Services { get; set; } = new();

    /// <summary>
    /// Paquetes más usados (top 5 + others).
    /// </summary>
    public PackageUsageResultDto Packages { get; set; } = new();

    /// <summary>
    /// Ingresos agrupados por semana.
    /// </summary>
    public List<WeeklyIncomeDto> WeeklyIncome { get; set; } = new();

    /// <summary>
    /// Ingresos agrupados por turno (Shift) y por Location.
    /// </summary>
    public List<ShiftIncomeDto> ShiftIncome { get; set; } = new();

    /// <summary>
    /// Ingresos agrupados por Location.
    /// </summary>
    public List<LocationIncomeDto> LocationIncome { get; set; } = new();
}

