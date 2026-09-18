using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIGREF.Common.Models;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Keycloak.Services;
using SIGREF.Infrastructure.Keycloak.Services.Auth;
using SIGREF.Infrastructure.Keycloak.Services.Auth.Keycloak;

namespace SIGREF.Infrastructure.Keycloak;

public static class DependencyInjection
{
    public static IServiceCollection AddKeycloakInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        // 
        services.Configure<KeycloakOptions>(
            configuration.GetSection(KeycloakOptions.SectionName));

        // 2. Registrar el HttpClient (Fundamental para que KeycloakClient funcione)
        services.AddHttpClient<IKeycloakClient, KeycloakClient>();

        // 3. Otros servicios
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

        return services;
    }
}