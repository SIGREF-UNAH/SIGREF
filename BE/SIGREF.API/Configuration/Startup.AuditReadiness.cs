using SIGREF.API.Audit.Extensions;
using SIGREF.API.Services.FhirUtils;

namespace SIGREF.API;

public partial class Startup
{
     private void AddAuditAndReadiness(IServiceCollection services)
    {
        services.AddAuditServices();

        // readiness HAPI
        //services.AddHttpClient();  esta en base , pero si falla es aqui.
        services.AddHostedService<HapiReadinessWaiter>();
    }
}
