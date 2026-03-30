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

        // SearchParameter initializers  uno por recurso FHIR
        services.AddScoped<BaseFhirSearchParameterInitializer, HealthcareServiceSearchParameterInitializer>();
        services.AddScoped<BaseFhirSearchParameterInitializer, PatientSearchParameterInitializer>();
 
        // Orquestador  recibe IEnumerable<BaseFhirSearchParameterInitializer> via DI
        services.AddScoped<FhirSearchParameterOrchestrator>();
    }
}
