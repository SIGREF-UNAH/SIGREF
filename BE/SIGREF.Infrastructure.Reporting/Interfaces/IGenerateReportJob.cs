namespace SIGREF.Infrastructure.Reporting.Interfaces;

public interface IGenerateReportJob
{
    // El método exacto que Hangfire va a ejecutar
    Task ExecuteAsync(Guid historyId, CancellationToken cancellationToken);
}