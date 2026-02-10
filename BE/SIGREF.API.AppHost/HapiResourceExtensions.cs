using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Postgres;

namespace SIGREF.API.AppHost;

/// <summary>
/// Representa un recurso de servidor HAPI FHIR (JPA Server Starter)
/// ejecutándose como contenedor Docker dentro de Aspire.
/// </summary>
public class HapiResource(string name)
    : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string PrimaryEndpointName = "http";

    /// <summary>
    /// Expresión de conexión base al endpoint FHIR del servidor.
    /// Ejemplo resultante:
    /// http://hapi:8080/fhir
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{PrimaryEndpointScheme}://" +
            $"{PrimaryEndpoint.Property(EndpointProperty.Host)}:" +
            $"{PrimaryEndpoint.Property(EndpointProperty.Port)}/fhir");

    private EndpointReference PrimaryEndpoint =>
        new(this, PrimaryEndpointName);

    private string PrimaryEndpointScheme => "http";
}

/// <summary>
/// Métodos de extensión para configurar HAPI FHIR Server
/// dentro de una aplicación distribuida con Aspire.
/// </summary>
public static class HapiResourceExtensions
{
    /// <summary>
    /// Agrega un servidor HAPI FHIR (JPA Server Starter) a la aplicación.
    ///
    /// IMPORTANTE:
    /// - Esta imagen es OFICIAL (hapiproject/hapi)
    /// - No es demo
    /// 
    /// </summary>
    public static IResourceBuilder<HapiResource> AddHapiFhir(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string image = "hapiproject/hapi",
        string tag = "latest")
    {
        var resource = new HapiResource(name);

        return builder
            .AddResource(resource)
            .WithImage(image, tag)
            .WithHttpEndpoint(
                port: port,
                targetPort: 8080,
                name: HapiResource.PrimaryEndpointName)
            .PublishAsContainer();
    }

    /// <summary>
    /// Configura PostgreSQL como base de datos para HAPI FHIR.
    ///
    /// Esta configuración:
    /// - Inyecta las variables estándar de Spring Boot
    /// - Usa el dialecto oficial de HAPI FHIR para PostgreSQL
    /// </summary>
    public static IResourceBuilder<HapiResource> WithPostgresDatabase(
        this IResourceBuilder<HapiResource> builder,
        IResourceBuilder<PostgresServerResource> postgresServer,
        IResourceBuilder<PostgresDatabaseResource> database,
        IResourceBuilder<ParameterResource> username,
        IResourceBuilder<ParameterResource> password)
    {
        if (database.Resource.Parent != postgresServer.Resource)
        {
            throw new InvalidOperationException(
                "The provided database does not belong to the supplied Postgres server.");
        }

        return builder
            .WithReference(postgresServer)
            .WithReference(database)

            // Spring Boot datasource configuration
            .WithEnvironment(
                "SPRING_DATASOURCE_URL",
                ReferenceExpression.Create(
                    $"jdbc:postgresql://{postgresServer.Resource.Name}:5432/{database.Resource.DatabaseName}"))
            .WithEnvironment("SPRING_DATASOURCE_USERNAME", username)
            .WithEnvironment("SPRING_DATASOURCE_PASSWORD", password)
            .WithEnvironment(
                "SPRING_DATASOURCE_DRIVER_CLASS_NAME",
                "org.postgresql.Driver")

            // Dialecto propio de HAPI FHIR (OBLIGATORIO)
            .WithEnvironment(
                "SPRING_JPA_PROPERTIES_HIBERNATE_DIALECT",
                "ca.uhn.fhir.jpa.model.dialect.HapiFhirPostgresDialect")

            // Espera a que PostgreSQL esté listo antes de iniciar HAPI
            .WaitFor(postgresServer);
    }

    /// <summary>
    /// Monta y ACTIVA el archivo application.yaml de HAPI FHIR.
    ///
    /// Spring Boot NO usa automáticamente archivos montados,
    /// por lo que es obligatorio definir SPRING_CONFIG_LOCATION.
    /// </summary>
    public static IResourceBuilder<HapiResource> WithConfigurationFile(
        this IResourceBuilder<HapiResource> builder,
        string localConfigPath)
    {
        return builder
            // Monta el YAML dentro del contenedor
            .WithBindMount(localConfigPath, "/configs/application.yaml")

            // Indica explícitamente a Spring Boot que use ese archivo
            .WithEnvironment(
                "SPRING_CONFIG_LOCATION",
                "file:///configs/application.yaml");
    }

    /// <summary>
    /// Habilita CORS en HAPI FHIR.
    ///
    /// NOTA:
    /// Para producción se recomienda definir CORS en application.yaml
    /// o en el reverse proxy (Traefik / Nginx).
    /// </summary>
    public static IResourceBuilder<HapiResource> WithCorsEnabled(
        this IResourceBuilder<HapiResource> builder,
        string allowedOrigins = "*")
    {
        return builder
            .WithEnvironment("HAPI_FHIR_CORS_ENABLED", "true")
            .WithEnvironment("HAPI_FHIR_CORS_ALLOWED_ORIGIN", allowedOrigins);
    }

    /// <summary>
    /// Configura el nivel de logging raíz de Spring Boot.
    ///
    /// Valores comunes:
    /// TRACE | DEBUG | INFO | WARN | ERROR
    /// </summary>
    public static IResourceBuilder<HapiResource> WithLogLevel(
        this IResourceBuilder<HapiResource> builder,
        string logLevel = "INFO")
    {
        return builder.WithEnvironment(
            "LOGGING_LEVEL_ROOT",
            logLevel);
    }
}
