using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting;            
using QuestPDF.Infrastructure;
using SIGREF.Infrastructure.Reporting.Services.Reports;                  // Requerido para LicenseType

namespace SIGREF.Infrastructure.Reporting;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddReportingInfrastructure(this IHostApplicationBuilder builder)
    {
        // Registramos el servicio para que Hangfire lo pueda encontrar
        builder.Services.AddScoped<IReportingService, ReportingService>();

        // Configuración global de QuestPDF (Licencia)
        QuestPDF.Settings.License = LicenseType.Community;

        return builder;
    }
}