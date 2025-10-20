using SIGREF.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

app.Run();
