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
            // Monta el directorio de índices Lucene en el contenedor.
            // La carpeta './hapi-lucene-data' se crea automáticamente si no existe.
            // En producción, considerar usar un volumen persistente en lugar de bind mount.
            //.WithBindMount("./hapi-lucene-data", "/tmp/lucenefiles")
            .PublishAsContainer();
    }

    /// <summary>
    /// Configura PostgreSQL como base de datos para HAPI FHIR.
    ///
    /// REGLA DE ORO: Aquí solo van propiedades de INFRAESTRUCTURA dinámica,
    /// es decir, valores que Aspire genera en tiempo de ejecución y que no
    /// podemos conocer de antemano en el YAML (URL, usuario, contraseña).
    ///
    /// TODO lo que es configuración estática de Hibernate/Spring (dialecto,
    /// Lucene, batching, etc.) vive en hapi.application.yaml, NO aquí.
    ///
    /// RAZÓN TÉCNICA: Mezclar variables de entorno UPPER_SNAKE_CASE de Aspire
    /// con propiedades YAML anidadas bajo 'hibernate:' causa que Spring Boot
    /// ignore el bloque anidado completo, rompiendo Hibernate Search 7.2.
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

            // URL dinámica: generada por Aspire en runtime según el nombre del contenedor Postgres.
            // No puede estar en YAML porque el hostname cambia en cada ejecución.
            .WithEnvironment(
                "SPRING_DATASOURCE_URL",
                ReferenceExpression.Create(
                    $"jdbc:postgresql://{postgresServer.Resource.Name}:5432/{database.Resource.DatabaseName}"))

            // Credenciales dinámicas: vienen de ParameterResource (secretos de Aspire).
            // Tampoco pueden estar en YAML por razones de seguridad.
            .WithEnvironment("SPRING_DATASOURCE_USERNAME", username)
            .WithEnvironment("SPRING_DATASOURCE_PASSWORD", password)

            // Dialecto propio de HAPI FHIR (OBLIGATORIO)
            //.WithEnvironment(
            //    "SPRING_JPA_PROPERTIES_HIBERNATE_DIALECT",
            //    "ca.uhn.fhir.jpa.model.dialect.HapiFhirPostgresDialect")

            // Espera a que PostgreSQL esté listo antes de iniciar HAPI.
            .WaitFor(postgresServer);
    }

    /// <summary>
    /// Monta y ACTIVA el archivo application.yaml de HAPI FHIR.
    ///
    /// Spring Boot NO usa automáticamente archivos montados,
    /// por lo que es obligatorio definir SPRING_CONFIG_ADDITIONAL_LOCATION.
    ///
    /// Se usa ADDITIONAL_LOCATION (no LOCATION) para FUSIONAR con el
    /// application.yaml interno de la imagen, no reemplazarlo.
    /// Nuestras propiedades tienen precedencia sobre las internas.
    /// </summary>
    public static IResourceBuilder<HapiResource> WithConfigurationFile(
        this IResourceBuilder<HapiResource> builder,
        string localConfigPath)
    {
        return builder
            // Monta el YAML dentro del contenedor.
            .WithBindMount(localConfigPath, "/configs/application.yaml")
            // ADDITIONAL_LOCATION fusiona configuraciones; nuestro YAML tiene precedencia.
            .WithEnvironment(
                "SPRING_CONFIG_ADDITIONAL_LOCATION",
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