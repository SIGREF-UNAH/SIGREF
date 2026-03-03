using SIGREF.API.Constants;

namespace SIGREF.API;

public partial class Startup
{
    private void ConfigureBase(IServiceCollection services)
    {
        services.Configure<Env>(_configuration);

        // Cache global para FHIR lookup
        services.AddMemoryCache();

        // Solo UNA vez
        services.AddHttpContextAccessor();

        //services.AddScoped<IUserContextService, UserContextService>()
    }

}
