using Microsoft.EntityFrameworkCore;
using SIGREF.API.ServiceDefaults;
using SIGREF.Common.Bridges;
using SIGREF.Common.Extensions;
using SIGREF.Common.Interfaces;
using SIGREF.Infrastructure.Reporting;
using SIGREF.Hangfire.Worker;
using SIGREF.Hangfire.Worker.Jobs;
using SIGREF.Infrastructure.Keycloak;
using SIGREF.Infrastructure.Persistence;
using SIGREF.Infrastructure.Reporting.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// 1. Infraestructura base de Aspire (Logging, OTEL, Service Discovery)
builder.AddServiceDefaults();
builder.Services.AddScoped<IGenerateReportJob, GenerateReportJob>();
// 2. Conexiones a Datos (Postgres y FHIR centralizado en Common)
builder.AddNpgsqlDataSource("sigref");
builder.Services.AddDbContext<SIGREFContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("sigref")));

builder.Services.AddKeycloakInfrastructure(builder.Configuration);
builder.Services.AddScoped<IFhirPractitionerService, FhirPractitionerBridge>();
builder.Services.AddFhirClientShared(builder.Configuration);

// 3. Configuración de Hangfire y Servicios de Reportes (Encapsulado)
builder.AddWorkerHangfire(); 
builder.AddReportingInfrastructure();
builder.Services.AddHttpContextAccessor();
// 4. Servicio en segundo plano
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();