using Hl7.Fhir.Rest;
using Microsoft.OpenApi.Models;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Services;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;

namespace SIGREF.API;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configurar las opciones de variables de entorno
        services.Configure<Env>(_configuration);

        // Registrar FhirClient directamente
        services.AddScoped<FhirService>();
        services.AddScoped<FhirClient>(serviceProvider =>
        {
            var fhirService = serviceProvider.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // Registrar servicios personalizados
        services.AddScoped<LocationService>();
        services.AddScoped<HealthcareService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerService, PractitionerService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();

        // Configuración de PostgreSQL con Aspire
        services.AddNpgsql<SIGREFContext>("hapi");

        // Configuración de Swagger para JWT
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese 'Bearer' seguido de un espacio y el token JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        services.AddHttpContextAccessor();
        services.AddNpgsql<SIGREFContext>("hapi");

        // CORS Configuration
        services.AddCors(opt =>
        {
            var allowURLS = _configuration.GetSection("AllowURLS").Get<string[]>();
            opt.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowURLS)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
    }
  
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
