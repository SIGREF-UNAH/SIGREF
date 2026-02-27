using SIGREF.API.Database;

namespace SIGREF.API;

public partial class Startup
{
    private void AddSeeders(IServiceCollection services)
    {
        services.AddScoped<RolesAdminSeeder>();
        services.AddScoped<TiposUbicacionSeeder>();
        services.AddScoped<SIGREFSeeder>();
    }
}
