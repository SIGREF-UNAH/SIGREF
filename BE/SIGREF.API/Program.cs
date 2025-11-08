using SIGREF.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Registrar servicios de AuditLog
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SIGREF.API.Services.AuditLog.IAuditLogService, SIGREF.API.Services.AuditLog.AuditLogService>();
var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

app.Run();
