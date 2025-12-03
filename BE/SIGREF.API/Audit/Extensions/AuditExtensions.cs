using SIGREF.API.Audit.Middleware;
using SIGREF.API.Audit.Services;

namespace SIGREF.API.Audit.Extensions;

public static class AuditExtensions
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }

    public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuditMiddleware>();
        return app;
    }
}
