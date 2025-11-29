using Aspire.Hosting.Yarp.Transforms;
using Projects;
using SIGREF.API.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// =============================================================
// POSTGRESQL - Instancia principal (HAPI, Keycloak, SIGREF)
// =============================================================
var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImage("postgres", "17")
    .WithEnvironment("POSTGRES_DB", builder.Configuration["Parameters:postgres-db"] ?? "postgres")
    .WithBindMount("./data/postgres", "/var/lib/postgresql/data")
    .WithBindMount("./config/init-db.sql", "/docker-entrypoint-initdb.d/init-db.sql")
    .WithHostPort(5432);

if (builder.ExecutionContext.IsRunMode)
{
    postgres.WithPgAdmin(builder =>
    {
        builder
            .WithImage("dpage/pgadmin4", "latest");
     });
}

var creationScript = "CREATE DATABASE {{databaseName}};";
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
    .WithBindMount("./data/mongo-sigref-logs", "/data/db")
    .AddDatabase("sigref-logs");

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
    .WithEnvironment("KC_HTTP_RELATIVE_PATH", "/keycloak")
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WaitFor(keycloakDb)
    .PublishAsContainer();

// =============================================================
// HAPI FHIR - Servidor de Datos Clínicos
// =============================================================
var hapi = builder
    .AddHapiFhir("hapi-fhir")
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword)
    .WithConfigurationFile("./config/hapi.application.yaml");

// =============================================================
// SIGREF.API - API Principal del Sistema para Receptoría de Fondos
// =============================================================
var sigrefApi = builder
     // .AddDockerfile("sigref-api", "../", "SIGREF.API/Dockerfile")
     .AddProject<SIGREF_API>("sigref-api")
    .WithReference(hapiDb)
    .WithReference(sigrefDb)
    .WithReference(mongoSigrefLogs)
    .WithReference(keycloak);


// =============================================================
// YARP - Reverse Proxy
// =============================================================
var yarpProxy = builder.AddYarp("gateway")
    .WithHostPort(5000)
    .WithConfiguration(yarp =>
    {
        // // Add catch-all route for frontend service
        // yarp.AddRoute(catalogService);

        // Add specific path route with transforms
        yarp.AddRoute("/api/{**catch-all}", sigrefApi);


        // Add specific path route with transforms
        yarp.AddRoute("/keycloak/{**catch-all}", keycloak);

        yarp.AddRoute("/hapi/fhir/{**catch-all}", hapi)
            .WithTransformPathRemovePrefix("/hapi");


   
    });

builder.Build().Run();