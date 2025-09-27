using SIGREF.API.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SIGREF.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SIGREF API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingrese 'Bearer' seguido de un espacio y el token JWT"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddHttpContextAccessor();
            services.AddNpgsql<SIGREFContext>("hapi");

            services.AddCors(opt =>
            {
                var allowURLS = _configuration.GetSection("AllowURLS").Get<string[]>();
                opt.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(allowURLS ?? new[] { "*" })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            var jwtKey = _configuration["Jwt:Key"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtAuthority = _configuration["Jwt:Authority"];

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

            if (_configuration.GetSection("Jwt").Exists())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}