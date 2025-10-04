using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.OpenApi.Models;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Services;
using SIGREF.API.Services.Location;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;
using System.Reflection;

namespace SIGREF.API;
/// <summary>
/// Clase principal de arranque de la aplicación ASP.NET Core.
/// Se encarga de configurar los servicios y el pipeline de ejecución.
/// </summary>
public class Startup
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor de la clase Startup.
    /// </summary>
    /// <param name="configuration">
    /// Objeto de configuración (IConfiguration) que contiene
    /// valores de appsettings.json, variables de entorno, etc.
    /// </param>
    public Startup(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    /// <summary>
    /// Método para registrar y configurar los servicios que estarán disponibles 
    /// a través de la inyección de dependencias.
    /// </summary>
    /// <param name="services">Colección de servicios (IServiceCollection).</param>
    /// <remarks>
    /// Aquí se registran:
    /// <list type="bullet">
    ///   <item><description>Opciones de configuración de entorno (<c>Env</c>).</description></item>
    ///   <item><description>Servicios FHIR (<c>FhirService</c>, <c>FhirClient</c>).</description></item>
    ///   <item><description>Servicios propios como <c>LocationService</c>.</description></item>
    ///   <item><description>Controladores y utilidades de API (Swagger, HttpContext).</description></item>
    ///   <item><description>Conexión a base de datos PostgreSQL (<c>SIGREFContext</c>).</description></item>
    ///   <item><description>Configuración de CORS (orígenes permitidos).</description></item>
    /// </list>
    /// </remarks>
    public void ConfigureServices(IServiceCollection services)
    {
        // --- Configuración de entorno (Env) ---
        // Se mapea la configuración completa proveniente de appsettings.json o variables de entorno
        // hacia la clase fuertemente tipada "Env". Esto permite acceder a parámetros de configuración,
        // como la URL base de FHIR (Env.Phir.BaseUrl), mediante inyección de dependencias (IOptions<Env>).
        services.Configure<Env>(_configuration);

        // --- Registro de servicios FHIR ---
        // Se registran los servicios necesarios para interactuar con un servidor FHIR:
        //
        // 1. FhirService:
        //    - Registrado con ciclo de vida Scoped (una instancia por cada request HTTP).
        //    - Encapsula la lógica de inicialización y configuración del cliente FHIR.
        //
        // 2. FhirClient:
        //    - También Scoped, pero creado a través de una factoría (lambda).
        //    - La factoría obtiene el FhirService desde el contenedor y utiliza su método
        //      GetFhirClient() para devolver una instancia ya configurada.
        //    - Esto garantiza que cualquier clase que requiera un FhirClient reciba
        //      un cliente listo para consumir el servidor FHIR, utilizando la configuración definida.
        services.AddScoped<FhirService>();
        services.AddScoped<FhirClient>(serviceProvider =>
        {
            var fhirService = serviceProvider.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // SEEDER
        services.AddScoped<SIGREFSeeder>();


        // Registrar FhirService (opcional si aún lo necesitas)
        services.AddScoped<LocationService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // Configuración de Swagger para JWT
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
        // --- Configuración de entorno (Env) ---
        // Se mapea la configuración completa proveniente de appsettings.json o variables de entorno
        // hacia la clase fuertemente tipada "Env". Esto permite acceder a parámetros de configuración,
        // como la URL base de FHIR (Env.Phir.BaseUrl), mediante inyección de dependencias (IOptions<Env>).
        services.Configure<Env>(_configuration);

        // --- Registro de servicios FHIR ---
        // Se registran los servicios necesarios para interactuar con un servidor FHIR:
        //
        // 1. FhirService:
        //    - Registrado con ciclo de vida Scoped (una instancia por cada request HTTP).
        //    - Encapsula la lógica de inicialización y configuración del cliente FHIR.
        //
        // 2. FhirClient:
        //    - También Scoped, pero creado a través de una factoría (lambda).
        //    - La factoría obtiene el FhirService desde el contenedor y utiliza su método
        //      GetFhirClient() para devolver una instancia ya configurada.
        //    - Esto garantiza que cualquier clase que requiera un FhirClient reciba
        //      un cliente listo para consumir el servidor FHIR, utilizando la configuración definida.
        services.AddScoped<FhirService>();
        services.AddScoped<FhirClient>(serviceProvider =>
        {
            var fhirService = serviceProvider.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // Registrar FhirService (opcional si aún lo necesitas)
        services.AddScoped<LocationService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerService, PractitionerService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
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

    /// <summary>
    /// Método para configurar el pipeline de procesamiento de la aplicación.
    /// </summary>
    /// <param name="app">Aplicación (IApplicationBuilder) usada para construir la tubería HTTP.</param>
    /// <param name="env">Entorno web (IWebHostEnvironment) que indica si es Desarrollo, Producción, etc.</param>
    /// <remarks>
    /// En este método se definen los middleware y el flujo de la aplicación:
    /// <list type="bullet">
    ///   <item><description>Swagger para documentación (solo en desarrollo).</description></item>
    ///   <item><description>Redirección HTTPS.</description></item>
    ///   <item><description>Ruteo (Routing).</description></item>
    ///   <item><description>Política de CORS aplicada.</description></item>
    ///   <item><description>Mapeo de controladores (<c>endpoints.MapControllers()</c>).</description></item>
    /// </list>
    /// </remarks>
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
