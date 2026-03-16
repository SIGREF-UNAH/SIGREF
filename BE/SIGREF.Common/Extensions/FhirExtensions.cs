using Hl7.Fhir.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
namespace SIGREF.Common.Extensions;

public static class FhirExtensions
{
    public static IServiceCollection AddFhirClientShared(this IServiceCollection services, IConfiguration configuration)
    {
        // Aspire inyecta los parámetros en la configuración
        // Intenta leer de la forma nueva de Aspire, y si no, de la variable directa (forma vieja)
        var url = configuration["Parameters:HAPIFHIR_HTTP"] ?? configuration["HAPIFHIR_HTTP"];
        
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("La URL de HAPI FHIR no está configurada en los parámetros de Aspire.");

        services.AddScoped<FhirClient>(sp =>
        {
            return new FhirClient($"{url}/fhir", new FhirClientSettings
            {
                Timeout = 60000, // Tiempo extendido para reportes pesados
                PreferredFormat = ResourceFormat.Json
            });
        });

        return services;
    }
}