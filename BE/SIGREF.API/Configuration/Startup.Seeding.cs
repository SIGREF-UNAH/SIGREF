using SIGREF.API.Database;
using SIGREF.API.Database.Seeding;

namespace SIGREF.API;

public partial class Startup
{
    private void AddSeeders(IServiceCollection services)
    {
        // Catálogos fijos
        services.AddScoped<ITerminologySeeder, RolesAdminSeeder>();

        // Catálogos que pueden recibir conceptos nuevos (IUpdatableTerminologySeeder)
        services.AddScoped<ITerminologySeeder, TiposUbicacionSeeder>();

        // Agregar nuevos catálogos aquí ejemplo
        // services.AddScoped<ITerminologySeeder, EspecialidadesMedicasSeeder>();

        // Orquestador — siempre al final
        services.AddScoped<SIGREFSeeder>();
    }
}
