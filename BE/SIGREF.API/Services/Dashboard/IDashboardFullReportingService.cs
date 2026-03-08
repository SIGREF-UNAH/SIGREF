using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Dashboard;

public interface IDashboardFullReportingService
{
    /// <summary>
    /// Obtiene TODA la información necesaria del Dashboard en una sola respuesta:
    /// - Summary (ingresos, servicios, pacientes, cierres con errores)
    /// - Servicios (top / bottom)
    /// - Paquetes (top 5 + otros)
    /// - Ingresos semanales
    /// - Ingresos por turno
    /// - Ingresos por location
    /// </summary>
    Task<ResponseDto<DashboardFullResultDto>> GetFullDashboardAsync(DashboardFilterDto filter);
}