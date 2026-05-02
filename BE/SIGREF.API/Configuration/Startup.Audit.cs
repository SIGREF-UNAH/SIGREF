using MongoDB.Driver;
using SIGREF.API.Audit.Middleware.Quee;
using SIGREF.API.Audit.Middleware.worker;
using SIGREF.API.Services.FhirUtils;
using SIGREF.API.Services.Hangfire;

namespace SIGREF.API;

public partial class Startup
{
    // 1. Registro de Servicios y Dependencias
    private void Audit(IServiceCollection services)
    {
        services.AddSingleton<IAuditQueue, AuditQueue>();
        services.AddHostedService<AuditWorker>();
        
    }
}