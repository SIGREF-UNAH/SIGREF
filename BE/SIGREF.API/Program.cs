using SIGREF.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = "https://localhost:8081/realms/fhir";
    options.Audience = "fhir-admin";
});
builder.Services.AddAuthorizationBuilder();
var app = builder.Build();

app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

app.Run();
