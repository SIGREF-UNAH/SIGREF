using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Services;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;
using System.Reflection;
using System.Text;

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
    ///   <item><description>Autenticación y autorización JWT.</description></item>
    /// </list>
    /// </remarks>
    public void ConfigureServices(IServiceCollection services)
    {
        // --- Configuración de entorno (Env) ---
        services.Configure<Env>(_configuration);

        // --- Registro de servicios FHIR ---
        services.AddScoped<FhirService>();
        services.AddScoped<FhirClient>(serviceProvider =>
        {
            var fhirService = serviceProvider.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // Registrar servicios de aplicación
        services.AddScoped<LocationService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerService, PractitionerService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // --- Configuración de Swagger ---
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SIGREF API",
                Version = "v1"
            });

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

        // --- Configuración de Base de Datos ---
        services.AddNpgsql<SIGREFContext>("hapi");

        // --- Configuración de CORS ---
        services.AddCors(opt =>
        {
            var allowURLS = _configuration.GetSection("AllowURLS").Get<string[]>();
            opt.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowURLS ?? new[] { "*" })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        // --- CONFIGURACIÓN JWT ---
        var jwtKey = _configuration["Jwt:Key"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtAuthority = _configuration["Jwt:Authority"];

        // Solo configurar autenticación JWT si hay configuración disponible
        if (!string.IsNullOrEmpty(jwtKey) || !string.IsNullOrEmpty(jwtAuthority))
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                if (!string.IsNullOrEmpty(jwtAuthority))
                {
                    // Configuración para Keycloak/OAuth
                    options.Authority = jwtAuthority;
                    options.Audience = jwtAudience;
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = !_configuration.GetValue<bool>("Development:AllowHttp");

                    // Configurar el claim type para roles si usas Keycloak
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        RoleClaimType = "role" // o "realm_access/roles" según tu configuración de Keycloak
                    };
                }
                else if (!string.IsNullOrEmpty(jwtKey))
                {
                    // Configuración para JWT con clave simétrica
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role" // Define donde están los roles en tu token
                    };
                }
            });

            // --- CONFIGURACIÓN DE AUTORIZACIÓN ---
            services.AddAuthorization(options =>
            {
                // Política básica para usuarios autenticados
                options.AddPolicy("Bearer", policy => policy.RequireAuthenticatedUser());

                // Políticas específicas por rol
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireRole("admin"));

                options.AddPolicy("PatientOnly", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireRole("patient"));

                options.AddPolicy("AdminOrPatient", policy =>
                    policy.RequireAuthenticatedUser()
                          .RequireRole("admin", "patient"));
            });
        }
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
    ///   <item><description>Autenticación y autorización JWT (si está configurada).</description></item>
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

        // --- MIDDLEWARE DE AUTENTICACIÓN CONDICIONAL ---
        // Solo usar autenticación si hay configuración JWT
        if (_configuration.GetSection("Jwt").Exists())
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}