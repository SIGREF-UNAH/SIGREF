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
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using SIGREF.API.Services.Auth;
using MongoDB.Driver;
using SIGREF.API.Services.AdministrationHospital;
using SIGREF.API.Services.Auth.Keycloak;
using SIGREF.API.Services.Cashier;
using SIGREF.API.Services.Files;


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
    // ================= ENV CONFIG ====================
    services.Configure<Env>(_configuration);

    // ================= FHIR CLIENT ====================
    services.AddScoped<FhirService>();
    services.AddScoped<FhirClient>(sp =>
    {
        var fhirService = sp.GetRequiredService<FhirService>();
        return fhirService.GetFhirClient();
    });

    // ================= SEEDERS =======================
    services.AddScoped<RolesAdminSeeder>();
    services.AddScoped<TiposUbicacionSeeder>();
    services.AddScoped<SIGREFSeeder>();

    // ================= HEALTH SERVICES ===============
    services.AddScoped<LocationService>();
    services.AddScoped<HealthcareService>();
    services.AddScoped<IPatientService, PatientService>();
    services.AddScoped<IPractitionerRoleService, PractitionerRoleService>();
    services.AddScoped<IPractitionerService, PractitionerService>();
    services.AddScoped<IOrganizationService, OrganizationService>();
    
    services.AddScoped<IUserContextService, UserContextService>();
    // ================ SIGREF SERVICES =======================
    services.AddScoped<IShiftService, ShiftService>();
    services.AddScoped<ICashierSessionService, CashierSessionService>();
    services.AddScoped<IHospitalPropertiesService, HospitalPropertiesService>();
    services.AddScoped<IMediaFileService, MediaFileService>();



    // ==============================================================
    //  KEYCLOAK CLIENT + ADMIN SERVICE 
    // ==============================================================

    // Cliente HTTP para Keycloak
    services.AddHttpClient<IKeycloakClient, KeycloakClient>();

    // Servicio administrador de Keycloak
    services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
    
    
    

    // ================= MVC / Swagger ===================
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddHttpContextAccessor();


    // ================= DATABASES ========================

    // SIGREF (PostgreSQL via Aspire)
    services.AddNpgsql<SIGREFContext>("sigref");
    
    
    services.AddScoped<IUserContextService, UserContextService>();


    // ================== MONGO LOGGING ==================
    services.Configure<MongoSettings>(_configuration.GetSection("Mongo"));

    services.AddSingleton<IMongoClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
        return new MongoClient(settings.ConnectionString);
    });

    services.AddSingleton(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
        var client = sp.GetRequiredService<IMongoClient>();
        return client.GetDatabase(settings.Database);
    });


    // ================== AUTHENTICATION ==================

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
            ValidIssuer = authority,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        // Mapear roles del token
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    var validRoles = new[]
                    {
                        RolesConstants.admin,
                        RolesConstants.cashier,
                        RolesConstants.ti,
                        RolesConstants.auditor
                    };

                    // Realm Access
                    var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccess))
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesArray))
                        {
                            foreach (var r in rolesArray.EnumerateArray())
                            {
                                var roleName = r.GetString();
                                if (validRoles.Contains(roleName))
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }

                    // Client Access
                    var resourceAccess = context.Principal.FindFirst("resource_access")?.Value;
                    if (!string.IsNullOrEmpty(resourceAccess))
                    {
                        using var doc = JsonDocument.Parse(resourceAccess);

                        if (doc.RootElement.TryGetProperty(audience, out var clientElement)
                            && clientElement.TryGetProperty("roles", out var clientRoles))
                        {
                            foreach (var r in clientRoles.EnumerateArray())
                            {
                                var roleName = r.GetString();
                                if (validRoles.Contains(roleName))
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
    

    // ================== CORS ==================
    services.AddCors(opt =>
    {
        var allowUrls = _configuration.GetSection("AllowURLS").Get<string[]>();
        opt.AddPolicy("CorsPolicy", builder => builder
            .WithOrigins(allowUrls)
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
