using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Services.Common;
using SIGREF.API.Services.Healthcare;
using SIGREF.API.Services.Location;
using SIGREF.API.Services.Organization;
using SIGREF.API.Services.Organizations;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;
using SIGREF.API.Services.PractitionerRole;
using SIGREF.API.Services.ServiceGroup;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using SIGREF.API.Services.Auth;
using MongoDB.Driver;
using SIGREF.API.Services.AdministrationHospital;
using SIGREF.API.Services.Auth.Keycloak;
using SIGREF.API.Services.Billing;
using SIGREF.API.Services.Cashier;
using SIGREF.API.Services.Dashboard;
using SIGREF.API.Services.FhirUtils;
using SIGREF.API.Services.Files;
using SIGREF.API.Services.Serie;
using SIGREF.API.Audit.Extensions;
using SIGREF.API.Helpers;


namespace SIGREF.API;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, WebApplicationBuilder applicationBuilder)
    {
        // Configurar las opciones de variables de entorno
        services.Configure<Env>(_configuration);
        
        // Cache global para el FHIR Lookup
        services.AddMemoryCache();


        // Registrar FhirClient directamente
        services.AddScoped<FhirService>();
        services.AddScoped<FhirClient>(serviceProvider =>
        {
            var fhirService = serviceProvider.GetRequiredService<FhirService>();
            return fhirService.GetFhirClient();
        });

        // SEEDER
        services.AddScoped<RolesAdminSeeder>();
        services.AddScoped<TiposUbicacionSeeder>();
        services.AddScoped<SIGREFSeeder>();

        // ================= HEALTH SERVICES ===============
        services.AddScoped<LocationService>();
        services.AddScoped<HealthcareFHIRService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerRoleService, PractitionerRoleService>();
        services.AddScoped<IPractitionerService, PractitionerService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ServiceGroupService>();
        
        // ============= RECUPERADORES FHIR ==========
        services.AddScoped<IFhirLookupService, FhirLookupService>();
        // Registrar dashboard que usa el lookup
        services.AddScoped<IDashboardReportingService, DashboardReportingService>();

        services.AddScoped<IUserContextService, UserContextService>();
        // ================ SIGREF SERVICES =======================
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ICashierSessionService, CashierSessionService>();
        services.AddScoped<IHospitalPropertiesService, HospitalPropertiesService>();
        services.AddScoped<IMediaFileService, MediaFileService>();
        services.AddScoped<ISerieService, SerieService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IHealthcareService, HealthcareApplicationService>();


        // ==============================================================
        //  KEYCLOAK CLIENT + ADMIN SERVICE 
        // ==============================================================
        // Resolucion a la cache de keycloak
        //  Registro del client primero
        services.AddHttpClient<KeycloakClient>(); // HttpClient factory
        // levantamiento de la interfaz utilizando el client resuelto 
        services.AddSingleton<IKeycloakClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(nameof(KeycloakClient));

            var config = sp.GetRequiredService<IConfiguration>();
            return new KeycloakClient(http, config);
        });
        // SERVICIO ADMINISTRADOR DE KEYCLOAK 
        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

        
        // Configuración de PostgreSQL con Aspire
        // ========================================================
        // NOTA: AddNpgsql ahora está en Program.cs donde debe estar en Aspire 9
        // Base de datos HAPI FHIR
        // No entiendo por que se enlazaba ese contexto aqui, si directamente se utiliza un client
        // el contexto es para tener acceso directo a la base de datos ejemplo contex.users
        //services.AddNpgsql<HapiContext>("hapi");
        // MongoDB is now configured via builder.AddMongoDBClient() in Program.cs
        // IMongoClient is automatically available via DI

        // ================= MVC / Swagger ===================
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SchemaFilter<EnumSchemaFilter>();
        });
        services.AddHttpContextAccessor();


        // ================= DATABASES ========================

        // SIGREF (PostgreSQL via Aspire)
        //services.AddNpgsql<SIGREFContext>("sigref");


        services.AddScoped<IUserContextService, UserContextService>();


        // ================== AUDIT SERVICES ==================
        services.AddAuditServices();
        // ================= HAPI READINESS ==================
        services.AddHttpClient();
        services.AddHostedService<HapiReadinessWaiter>();
        services.AddScoped<HealthcareServiceSearchParameterInitializer>();


        services.AddHttpContextAccessor();

        services.AddAuthentication()
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: _configuration["Keycloak:RealmName"],
                options =>
                {
                    options.Audience = _configuration["Keycloak:Audience"];
                    options.RequireHttpsMetadata = false;
                    options.Authority = _configuration["Keycloak:Authority"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        RoleClaimType = ClaimTypes.Role
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity == null) return Task.CompletedTask;

                            // Roles válidos dentro del sistema
                            var validRoles = new[]
                            {
                                RolesConstants.cashier, // "cashier"
                                RolesConstants.admin, // "admin"
                                RolesConstants.ti, // "ti"
                                RolesConstants.auditor // "auditor"
                            };

                            // ======================================================
                            //  EXTRAER ROLES DE REALM
                            // ======================================================
                            var realmAccessClaim = context.Principal?.Claims
                                .FirstOrDefault(c => c.Type == "realm_access");

                            if (realmAccessClaim != null)
                            {
                                using var doc = JsonDocument.Parse(realmAccessClaim.Value);

                                if (doc.RootElement.TryGetProperty("roles", out var roles))
                                {
                                    foreach (var role in roles.EnumerateArray())
                                    {
                                        var roleName = role.GetString()?.ToLower();
                                        if (!string.IsNullOrEmpty(roleName) &&
                                            validRoles.Contains(roleName))
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                        }
                                    }
                                }
                            }

                            // ======================================================
                            // EXTRAER ROLES DE CLIENTE (resource_access)
                            // ======================================================
                            var resourceAccessClaim = context.Principal?.Claims
                                .FirstOrDefault(c => c.Type == "resource_access");

                            if (resourceAccessClaim != null)
                            {
                                using var doc = JsonDocument.Parse(resourceAccessClaim.Value);

                                if (doc.RootElement.TryGetProperty(options.Audience, out var client) &&
                                    client.TryGetProperty("roles", out var clientRoles))
                                {
                                    foreach (var role in clientRoles.EnumerateArray())
                                    {
                                        var roleName = role.GetString()?.ToLower();
                                        if (!string.IsNullOrEmpty(roleName) &&
                                            validRoles.Contains(roleName))
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                        }
                                    }
                                }
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

        services.AddAuthorization();

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

        app.UseCors("CorsPolicy");

        app.UseRouting();

        // Middleware de auditoría (después de routing, antes de auth)
        app.UseAuditMiddleware();

        //
        var mediaPath = Path.Combine(env.ContentRootPath, "media");
        if (!Directory.Exists(mediaPath))
            Directory.CreateDirectory(mediaPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mediaPath),
            RequestPath = "/media"
        });


        app.UseAuthentication();

        app.UseAuthorization();


        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}