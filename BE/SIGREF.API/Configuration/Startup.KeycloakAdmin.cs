using SIGREF.API.Services.Auth;
using SIGREF.API.Services.Auth.Keycloak;

namespace SIGREF.API;

public partial class Startup
{
    private void AddKeycloakAdmin(IServiceCollection services)
    {
        services.AddHttpClient<KeycloakClient>(); // factory

        services.AddSingleton<IKeycloakClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(nameof(KeycloakClient));

            var config = sp.GetRequiredService<IConfiguration>();
            return new KeycloakClient(http, config);
        });

        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
    }
}
