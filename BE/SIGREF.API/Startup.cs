using System.Reflection;
using SIGREF.API.Database;
using SIGREF.API.Constants;
using SIGREF.API.Services;
using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

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
            // Configurar las opciones de Env - inyectar la sección completa
            services.Configure<Env>(_configuration);

            // Registrar FhirClient directamente
            services.AddScoped<FhirService>();
            services.AddScoped<FhirClient>(serviceProvider =>
            {
                var fhirService = serviceProvider.GetRequiredService<FhirService>();
                return fhirService.GetFhirClient();
            });

            // Registrar FhirService (opcional si aún lo necesitas)
            services.AddScoped<LocationService>();
            
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpContextAccessor();

            // Configuración de PostgreSQL con Aspire
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
