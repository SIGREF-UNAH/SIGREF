
using Microsoft.OpenApi.Writers;
using SIGREF.API;
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8081/realms/fhir";
        options.RequireHttpsMetadata = false;
        options.Audience = "SIGREF-Api";
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidAudience = "SIGREF-Api",
            NameClaimType = "preferred_username",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorizationBuilder();
var app = builder.Build();
app.MapDefaultEndpoints();
startup.Configure(app, app.Environment);
app.Run();