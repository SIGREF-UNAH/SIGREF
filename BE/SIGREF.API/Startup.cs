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
using Microsoft.Extensions.Options;
using SIGREF.API.Services.Auth;
using MongoDB.Driver;


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

        // SEEDER
        services.AddScoped<RolesAdminSeeder>();
        services.AddScoped<TiposUbicacionSeeder>();
        services.AddScoped<SIGREFSeeder>();

        // Registrar FhirService (opcional si aún lo necesitas)
        services.AddScoped<LocationService>();
        services.AddScoped<HealthcareService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPractitionerRoleService, PractitionerRoleService>();
        services.AddScoped<IPractitionerService, PractitionerService>();
        services.AddScoped<IOrganizationService, OrganizationService>();

        services.AddScoped<ServiceGroupService>();

        services.AddScoped<KeycloakAdminService>();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();

        // Configuración de PostgreSQL con Aspire
        // ========================================================
        // Base de datos SIGREF (Gestion de Receptoraa de Fondos)
        services.AddNpgsql<SIGREFContext>("sigref");
        // Base de datos HAPI FHIR
        // No entiendo por que se enlazaba ese contexto aqui, si directamente se utiliza un client
        // el contexto es para tener acceso directo a la base de datos ejemplo contex.users
        //services.AddNpgsql<HapiContext>("hapi");

        // MongoDB is now configured via builder.AddMongoDBClient() in Program.cs
        // IMongoClient is automatically available via DI

        // Optional: Register IMongoDatabase if needed by services
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("sigref-logs");
        });

        services.AddHttpContextAccessor();

        // Configuración de Autenticación con Keycloak
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var authority = _configuration["Keycloak:Authority"];
            var audience = _configuration["Keycloak:Audience"];
            var requireHttps = _configuration.GetValue<bool>("Keycloak:RequireHttps");
        
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = requireHttps;
        
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidIssuer = $"{authority}",
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };
        
            // Aquí mapeamos los roles
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var identity = context.Principal.Identity as ClaimsIdentity;
        
                    if (identity != null)
                    {
                        // Lista de roles
                        var validRoles = new[] { 
                            RolesConstants.admin, 
                            RolesConstants.cashier, 
                            RolesConstants.ti, 
                            RolesConstants.auditor 
                        };
        
                        // --- Roles de Realm ---
                        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                        if (!string.IsNullOrEmpty(realmAccess))
                        {
                            using var doc = JsonDocument.Parse(realmAccess);
                            if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (validRoles.Contains(roleName))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
        
                        // --- Roles del Client ---
                        var resourceAccess = context.Principal.FindFirst("resource_access")?.Value;
                        if (!string.IsNullOrEmpty(resourceAccess))
                        {
                            using var doc = JsonDocument.Parse(resourceAccess);
                            if (doc.RootElement.TryGetProperty(audience, out var clientElement) &&
                                clientElement.TryGetProperty("roles", out var clientRoles))
                            {
                                foreach (var role in clientRoles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (validRoles.Contains(roleName))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        
        });
        //
        // services.AddAuthentication()
        //     .AddKeycloakJwtBearer("keycloak", realm: _configuration["Keycloak:RealmName"],
        //         options =>
        //         {
        //             options.RequireHttpsMetadata = _configuration.GetValue<bool>("Keycloak:RequireHttps");
        //             options.Audience = _configuration["Keycloak:Audience"];
        //             
        //             // IMPORTANTE: Forzar la URL interna para la validación de metadatos
        //             // Esto evita el error 404 al intentar contactar a Keycloak
        //             options.MetadataAddress = "http://keycloak-server:8080/keycloak/realms/sigref/.well-known/openid-configuration";
        //
        //             options.TokenValidationParameters = new TokenValidationParameters
        //             {
        //                 ValidateIssuer = true,
        //                 ValidateAudience = true,
        //                 ValidAudience = options.Audience,
        //                 NameClaimType = "preferred_username",
        //                 RoleClaimType = ClaimTypes.Role,
        //                 // Validar firma aunque no se pueda descargar el JWKS automáticamente si falla la conexión
        //                 ValidateIssuerSigningKey = true
        //             };
        //         });
        //
        // services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        //     .PostConfigure(options =>
        //     {
        //         // Configurar ValidIssuers para aceptar tanto URLs internas como externas
        //         var validIssuers = new List<string>();
        //         
        //         var realm = _configuration["Keycloak:RealmName"];
        //         // URL Interna (Docker)
        //         validIssuers.Add($"http://keycloak-server:8080/keycloak/realms/{realm}");
        //         // URL Externa (Directa)
        //         validIssuers.Add($"http://localhost:8081/keycloak/realms/{realm}");
        //         // URL Externa (YARP)
        //         validIssuers.Add($"http://localhost:5000/keycloak/realms/{realm}");
        //         // URL HTTPS (si aplica)
        //         validIssuers.Add($"https://localhost:8081/keycloak/realms/{realm}");
        //
        //         options.TokenValidationParameters.ValidIssuers = validIssuers;
        //         options.TokenValidationParameters.ValidIssuer = null;
        //     });
        // services.AddAuthorization();

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

        // app.UseHttpsRedirection();

        app.UseCors("CorsPolicy");

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();


        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}