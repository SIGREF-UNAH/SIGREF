using SIGREF.API.Helpers;

namespace SIGREF.API;

public partial class Startup
{
    private void AddControllersAndSwagger(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.SchemaFilter<EnumSchemaFilter>());
    }
}
