using System;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Postgres;

namespace SIGREF.API.AppHost;

/// <summary>
/// Representa un recurso de HAPI FHIR Server
/// </summary>
public class HapiResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string PrimaryEndpointName = "http";

   

    /// <summary>
    /// Obtiene la cadena de conexión para acceder al servidor HAPI FHIR
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{PrimaryEndpointScheme}://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}/fhir");

    private EndpointReference PrimaryEndpoint => new(this, PrimaryEndpointName);

    private string PrimaryEndpointScheme => "http";
}

/// <summary>
/// Métodos de extensión para configurar HAPI FHIR Server en Aspire
/// </summary>
public static class HapiResourceExtensions
{
    /// <summary>
    /// Agrega un servidor HAPI FHIR a la aplicación
    /// </summary>
    /// <param name="builder">El builder de la aplicación distribuida</param>
    /// <param name="name">El nombre del recurso</param>
    /// <param name="port">El puerto HTTP (opcional)</param>
    /// <param name="image">La imagen Docker a utilizar (por defecto: hapiproject/hapi)</param>
    /// <param name="tag">La etiqueta de la imagen (por defecto: latest)</param>
    /// <returns>Un resource builder para HAPI FHIR</returns>
    public static IResourceBuilder<HapiResource> AddHapiFhir(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string image = "hapiproject/hapi",
        string tag = "latest")
    {
        var resource = new HapiResource(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(image, tag)
            .WithHttpEndpoint(port: port, targetPort: 8080, name: HapiResource.PrimaryEndpointName)
            .PublishAsContainer();

        return resourceBuilder;
    }

    /// <summary>
    /// Configura la base de datos PostgreSQL para HAPI FHIR
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
            throw new InvalidOperationException("The provided database does not belong to the supplied Postgres server.");
        }

        return builder
            .WithReference(postgresServer)
            .WithReference(database)
            .WithEnvironment("JAVA_TOOL_OPTIONS", "-Djava.net.preferIPv4Stack=true")
            .WithEnvironment("SPRING_DATASOURCE_URL",
               ReferenceExpression.Create($"jdbc:postgresql://{postgresServer.Resource.Name}:5432/{database.Resource.DatabaseName}"))
            .WithEnvironment("SPRING_DATASOURCE_USERNAME", username)
            .WithEnvironment("SPRING_DATASOURCE_PASSWORD", password)
            .WithEnvironment("SPRING_DATASOURCE_DRIVER_CLASS_NAME", "org.postgresql.Driver")
            .WithEnvironment("SPRING_JPA_PROPERTIES_HIBERNATE_DIALECT", "ca.uhn.fhir.jpa.model.dialect.HapiFhirPostgresDialect")
            .WaitFor(postgresServer);
    }

    /// <summary>
    /// Configura el archivo de configuración de HAPI FHIR
    /// </summary>
    public static IResourceBuilder<HapiResource> WithConfigurationFile(
        this IResourceBuilder<HapiResource> builder,
        string configPath)
    {
        return builder.WithBindMount(configPath, "/app/config/application.yaml");
    }

    /// <summary>
    /// Habilita CORS en HAPI FHIR
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
    /// Configura el nivel de log de HAPI FHIR
    /// </summary>
    public static IResourceBuilder<HapiResource> WithLogLevel(
        this IResourceBuilder<HapiResource> builder,
        string logLevel = "INFO")
    {
        return builder.WithEnvironment("SPRING_LOG_LEVEL", logLevel);
    }
}

