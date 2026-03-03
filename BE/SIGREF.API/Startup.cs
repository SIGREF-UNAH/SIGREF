using SIGREF.API.Audit.Extensions;

namespace SIGREF.API;

public partial class Startup
{
    private readonly IConfiguration _configuration;
    private WebApplicationBuilder _builder;

    public Startup(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, WebApplicationBuilder applicationBuilder)
    {
        this._builder = applicationBuilder;
        ConfigureBase(services);             // Env, cache, httpcontext
        AddFhir(services);                   // FhirService + FhirClient
        AddSeeders(services);                // Seeders
        AddDomainServices(services, applicationBuilder);         // Health + SIGREF + Reportes + PDFs
        AddKeycloakAdmin(services);          // HttpClient + KeycloakClient + AdminService
        AddControllersAndSwagger(services);  // MVC + Swagger + filters
        AddAuditAndReadiness(services);      // Audit + HostedService + readiness
        AddAuth(services);                   // tu AddAuth parcial
        services.AddAuthorization();         // política general de authorization
        AddHangfire(services);               // Hangfire
        AddCorsPolicy(services);             // CORS
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("CorsPolicy");

        app.UseRouting();

        // Middleware de auditoría (después de routing, antes de auth)
        app.UseAuditMiddleware();

            // Carpetas de Media
        UseMediaStaticFiles(app, env);


        app.UseAuthentication();

        app.UseAuthorization();

        if (_configuration.GetValue<bool>("Hangfire:DashboardEnabled"))
        {
            UseHangfireDashboard(app);
        }

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}