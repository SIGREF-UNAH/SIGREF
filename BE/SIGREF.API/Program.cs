using SIGREF.API;
using Serilog;

// Configurar Serilog desde appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Usar Serilog como logger
builder.Host.UseSerilog();

builder.AddServiceDefaults();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

app.Run();
