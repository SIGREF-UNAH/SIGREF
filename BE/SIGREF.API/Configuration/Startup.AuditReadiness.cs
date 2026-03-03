using SIGREF.API.Audit.Extensions;
using SIGREF.API.Services.FhirUtils;
using SIGREF.API.Services.Hangfire;

namespace SIGREF.API;

public partial class Startup
{
     private void AddAuditAndReadiness(IServiceCollection services)
    {
        services.AddAuditServices();

        // readiness HAPI
        //services.AddHttpClient();  esta en base , pero si falla es aqui.
        services.AddHostedService<HapiReadinessWaiter>();

        // readiness Hangfire (se ejecutará incluso cuando el servidor de Hangfire
        // esté deshabilitado; el servicio internamente comprueba la cadena de
        // conexión y sale si falta)
        services.AddHostedService<HangfireReadinessWaiter>();
    }
}
