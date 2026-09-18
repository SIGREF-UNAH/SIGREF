using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF;
using QuestPDF.Infrastructure;
using SIGREF.Infrastructure.Reporting.Interfaces;
using SIGREF.Infrastructure.Reporting.Services;

namespace SIGREF.Infrastructure.Reporting;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddReportingInfrastructure(this IHostApplicationBuilder builder)
    {
        //Servicios principales 
        builder.Services.AddScoped<IReportDataCollector, ReportDataCollector>();
        builder.Services.AddScoped<IReportPdfBuilder, QuestPdfBuilder>();
        builder.Services.AddScoped<IReportQueueService, ReportQueueService>();

        // ─ Almacenamiento de PDFs
        // Configurable desde appsettings.json → "ReportStorage": { "BasePath": "/ruta" }
        builder.Services.Configure<ReportStorageOptions>(
            builder.Configuration.GetSection(ReportStorageOptions.SectionName));
        builder.Services.AddSingleton<IReportStorageService, LocalReportStorageService>();


        // QuestPDF 
        Settings.License = LicenseType.Community;

        return builder;
    }
}