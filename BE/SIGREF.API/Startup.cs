using Microsoft.Extensions.FileProviders;
using SIGREF.API.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;

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
        services.AddLocalization(options => options.ResourcesPath = "Resourses");
        Audit(services);
        ConfigureBase(services);             // Env, cache, httpcontext
        AddFhir(services);                   // FhirService + FhirClient
        AddSeeders(services);                // Seeders
        AddDomainServices(services, applicationBuilder);         // Health + SIGREF + Reportes + PDFs
        //AddKeycloakAdmin(services);          // HttpClient + KeycloakClient + AdminService Configuracion mediante la libreria
        AddControllersAndSwagger(services);  // MVC + Swagger + filters
        AddAuditAndReadiness(services);      // Audit + HostedService + readiness
        AddAuth(services);                   // tu AddAuth parcial
        services.AddAuthorization();         // política general de authorization
        AddHangfire(services);               // Hangfire
        AddCorsPolicy(services);             // CORS
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //Middleware temporal para debuggear Swagger
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/swagger"))
                {
                    try
                    {
                        await next();
                    }
                    catch (Exception ex)
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync($"ERROR SWAGGER: {ex.Message}\n\n{ex.StackTrace}");
                    }
                }
                else
                {
                    await next();
                }
            });
        
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGREF API v1");
    
                // Colapsar grupos por defecto (como HAPI FHIR)
                c.DocExpansion(DocExpansion.List);
    
                //  Activar barra de búsqueda/filtro nativa (filtra por tag, path, summary)
                c.EnableFilter();
                //  Profundidad de modelos
                c.DefaultModelsExpandDepth(1);
                c.DefaultModelExpandDepth(1);
    
                // UX adicional
                c.DisplayRequestDuration();
                c.DisplayOperationId();
                c.EnableDeepLinking();
                // (Opcional) Inyectar JS para ordenamiento custom si lo necesitas
                // c.InjectJavascript("/swagger-ui/custom-sort.js", "text/javascript");
            });
        }
       
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseMiddleware<AuditMiddleware>();
        // Middleware de auditoría (después de routing, antes de auth)
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