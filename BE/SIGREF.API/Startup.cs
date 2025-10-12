using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Services;
using SIGREF.API.Services.Location;
using SIGREF.API.Services.Organization;
using SIGREF.API.Services.Organizations;
using SIGREF.API.Services.Patient;
using SIGREF.API.Services.Practitioner;
using SIGREF.API.Services.PractitionerRole;
using System.Security.Claims;
using System.Text.Json;


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

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();

        // Configuración de PostgreSQL con Aspire
        services.AddNpgsql<SIGREFContext>("hapi");
        services.AddHttpContextAccessor();

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
                ValidAudiences = new[] { audience, "account" },
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = "preferred_username"
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
                            Roles.admin,
                            Roles.cashier,
                            Roles.ti,
                            Roles.auditor
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

                        // --- Roles del Client (sigref-api) ---
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
        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
