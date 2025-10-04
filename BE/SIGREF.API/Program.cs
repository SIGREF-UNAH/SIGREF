using SIGREF.API;
using SIGREF.API.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<SIGREFSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la inicializacion del seeder de terminologia FHIR.");
        throw; 
    }
}

app.Run();
