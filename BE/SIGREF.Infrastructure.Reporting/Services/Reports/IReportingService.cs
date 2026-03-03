namespace SIGREF.Infrastructure.Reporting.Services.Reports;

public interface IReportingService
{
    Task GenerarReportePacienteFactura(int facturaId, string pacienteId);
}
