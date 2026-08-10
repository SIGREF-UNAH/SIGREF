using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using SIGREF.API.Middleware;

namespace SIGREF.API;

public partial class Startup
{
    private readonly IConfiguration _configuration;
    private WebApplicationBuilder _builder;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, WebApplicationBuilder applicationBuilder)
    {
        _builder = applicationBuilder;
        services.AddLocalization(options => options.ResourcesPath = "Resourses");
        Audit(services);
        ConfigureBase(services);
        AddFhir(services);
        AddSeeders(services);
        AddDomainServices(services, applicationBuilder);
        AddControllersAndOpenApi(services);
        AddAuditAndReadiness(services);
        AddAuth(services);
        services.AddAuthorization();
        AddHangfire(services);
        AddCorsPolicy(services);
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseMiddleware<AuditMiddleware>();
        UseMediaStaticFiles(app, env);
        app.UseAuthentication();
        app.UseAuthorization();

        if (_configuration.GetValue<bool>("Hangfire:DashboardEnabled"))
        {
            UseHangfireDashboard(app);
        }

        if (env.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .WithTitle("SIGREF API"));
        }

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
