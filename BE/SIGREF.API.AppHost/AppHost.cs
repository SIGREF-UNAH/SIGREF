using Projects;
using SIGREF.API.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env");

// =============================================================
// POSTGRESQL - Instancia principal (HAPI, Keycloak, SIGREF)
// =============================================================
var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
        .WithImage("postgres", "17-alpine")
        .WithEnvironment("POSTGRES_DB", builder.Configuration["Parameters:postgres-db"] ?? "postgres")
        .WithDataVolume("data-postgres", isReadOnly: false)
        .WithHostPort(5432)
        .WithChildRelationship(postgresPassword)
        .WithChildRelationship(postgresUsername)

    // .WithInitFiles("./config/init-db.sql")
    ;

if (builder.ExecutionContext.IsRunMode)
{
    postgres.WithPgAdmin(builder =>
    {
        builder
            .WithImage("dpage/pgadmin4", "latest")
            ;
    });
}

var creationScript = "CREATE DATABASE  {{databaseName}};";
var hapiDb = postgres.AddDatabase("hapi")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "hapi"));
var sigrefDb = postgres.AddDatabase("sigref")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "sigref"));
var keycloakDb = postgres.AddDatabase("keycloakDb", "keycloak")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "keycloak"));

// =============================================================
// MONGODB - Base de datos para logs internos de SIGREF
// =============================================================
var mongoUser = builder.AddParameter("mongodb-user");
var mongoPassword = builder.AddParameter("mongodb-password", secret: true);

var mongoPort = 27017;

var mongoSigrefLogs = builder.AddMongoDB("mongo-sigref-logs", mongoPort, mongoUser, mongoPassword)
    .WithDataVolume("data-mongo-sigref-logs")
    .WithChildRelationship(mongoUser)
    .WithChildRelationship(mongoPassword);

var mongoDb = mongoSigrefLogs.AddDatabase("sigref-logs");


if (builder.ExecutionContext.IsRunMode)
{
    mongoSigrefLogs
     .WithMongoExpress();
}

// =============================================================
// KEYCLOAK - Servidor de Autenticación
// =============================================================
var keyCloakUser = builder.AddParameter("keycloak-user");
var keyCloakPass = builder.AddParameter("keycloak-pass", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8080, keyCloakUser, keyCloakPass)
    .WithReference(keycloakDb)
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", keycloakDb.Resource.JdbcConnectionString)
    .WithEnvironment("KC_DB_USERNAME", postgresUsername)
    .WithEnvironment("KC_DB_PASSWORD", postgresPassword)
    .WithEnvironment("KC_HOSTNAME", "localhost")
    .WithChildRelationship(keyCloakUser)
    .WithChildRelationship(keyCloakPass)
    .WaitFor(keycloakDb);

// En modo publicación, usar imagen personalizada con temas
if (builder.ExecutionContext.IsPublishMode)
{
    keycloak.WithDockerfile("./config", "Keycloak.Dockerfile");
}
// En desarrollo, montar temas como volumen
else
{
    keycloak.WithBindMount("./config/themes", "/opt/keycloak/themes");
}

keycloak.PublishAsContainer();



// =============================================================
// HAPI FHIR - Servidor de Datos Clínicos
// =============================================================
var hapi = builder
        .AddHapiFhir("hapifhir")
        .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword)
    ;

// =============================================================
// SIGREF.API - API Principal del Sistema para Receptoría de Fondos
// =============================================================
var sigrefApi = builder
        .AddProject<SIGREF_API>("sigref-api")
        .WithReference(sigrefDb)
        .WithReference(mongoDb)
        .WithReference(keycloak)
        .WithReference(hapi)
        .WaitFor(sigrefDb)
        .WaitFor(mongoDb)
        .WaitFor(hapi)
        .WaitFor(keycloak)
        .WithReferenceRelationship(postgres)
        .WithReferenceRelationship(mongoDb)
        .WithReferenceRelationship(hapi)
        .WithReferenceRelationship(keycloak)
        //set a port for the API
        // .WithEnvironment("ASPNETCORE_URLS", "http://+:5000")
        .WithHttpEndpoint( port: 5000, name: "http-dev")
    ;

// Cuando se usa deploy, Aspire automáticamente usa el Dockerfile
// No necesitamos configuración adicional

// =============================================================
// FRONTEND - Aplicación React + Vite
// =============================================================
IResourceBuilder<ContainerResource> frontend;

if (builder.ExecutionContext.IsRunMode)
{
    // En desarrollo: Montar el código fuente para hot-reload
  frontend = builder.AddContainer("frontend", "node", "20-alpine")
        .WithBindMount("../../FE", "/app")
        .WithEntrypoint("/bin/sh")
        .WithArgs("-c", "cd /app && npm install && npm run dev -- --host 0.0.0.0")
        .WithEnvironment("VITE_KEYCLOAK_URL", $"http://localhost:{keycloak.GetEndpoint("http").TargetPort.ToString()}")
        .WithEnvironment("VITE_KEYCLOAK_REALM", "sigref")
        .WithEnvironment("VITE_KEYCLOAK_CLIENT_ID", "frontend")
        .WithEnvironment("VITE_API_URL", $"http://localhost:5000")
        // .WithEnvironment("ORVAL_API_URL","http://localhost:5226/swagger/v1/swagger.json")
        .WithEnvironment("BROWSER", "none")
        .WithHttpEndpoint(targetPort: 5173, port: 5173, name: "http")
        .WaitFor(sigrefApi)
        .WaitFor(keycloak)
        .WithExternalHttpEndpoints();
}
else
{
    // En producción: Construir imagen Docker optimizada
    frontend = builder.AddContainer("frontend", "frontend")
        .WithDockerfile("../../FE", "Dockerfile")
        .WithBuildArg("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
        .WithBuildArg("VITE_KEYCLOAK_REALM", "sigref")
        .WithBuildArg("VITE_KEYCLOAK_CLIENT_ID", "frontend")
        .WithBuildArg("VITE_API_URL", sigrefApi.GetEndpoint("http"))
        .WithHttpEndpoint(targetPort: 80, port: 5173, name: "http")
        .WaitFor(sigrefApi)
        .WaitFor(keycloak)
        .WithExternalHttpEndpoints();

    frontend.PublishAsContainer();
}


builder.Build().Run();