using Hl7.Fhir.Rest;
using SIGREF.API.Services.Common;
using SIGREF.API.Services.Dashboard;
using SIGREF.API.Services.FhirUtils;

namespace SIGREF.API;

public partial class Startup
{
    private void AddFhir(IServiceCollection services)
    {
        services.AddScoped<FhirService>();

        services.AddScoped<FhirClient>(sp =>
        {
            var fhirService = sp.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // Lookup + dashboard reporting
        services.AddScoped<IFhirLookupService, FhirLookupService>();
        services.AddScoped<IDashboardReportingService, DashboardReportingService>();

        // inicializador
        services.AddScoped<HealthcareServiceSearchParameterInitializer>();
    }
}
