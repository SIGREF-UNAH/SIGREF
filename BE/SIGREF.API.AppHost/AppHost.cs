using DotNetEnv;
using SIGREF.API.AppHost;

// -------------------------------------------------------------
// Carga de variables de entorno (.env)
// -------------------------------------------------------------
Env.Load("./config/apphost.env"); // Carga todas las variables del archivo .env

var builder = DistributedApplication.CreateBuilder(args);

// =============================================================
// TRAEFIK - Reverse Proxy y Seguridad de Entrada (DESACTIVADO TEMPORALMENTE)
// =============================================================
// TODO:
// Traefik era el punto de entrada principal (gateway). Por el momento
// se ha deshabilitado esta configuración para ejecutar el sistema sin proxy
// Si en el futuro se requiere reactivar, descomentar este bloque.
// =============================================================

/*
var traefik = builder.AddContainer("traefik", "traefik", "latest")
    .WithEnvironment("TRAEFIK_API_INSECURE", Environment.GetEnvironmentVariable("TRAEFIK_DASHBOARD"))
    .WithEnvironment("TRAEFIK_PROVIDERS_DOCKER", "true")
    .WithEnvironment("TRAEFIK_ENTRYPOINTS_WEB_ADDRESS", ":80")
    .WithEnvironment("TRAEFIK_ENTRYPOINTS_WEBSECURE_ADDRESS", ":443")
    .WithBindMount("./config/traefik_static.yml", "/etc/traefik/traefik.yml")
    .WithBindMount("./config/traefik_dynamic.yml", "/etc/traefik/dynamic/dynamic.yml")
    .WithBindMount("./config/certs", "/etc/traefik/certs")
    .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
    .WithHttpEndpoint(port: 80, targetPort: 80, name: "web")
    .WithHttpEndpoint(port: 443, targetPort: 443, name: "websecure");
*/

// =============================================================
// POSTGRESQL - Instancia principal (HAPI, Keycloak, SIGREF)
// =============================================================
var postgresUsername = builder.AddParameter("postgres-username", Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "sigref");
var postgresPassword = builder.AddParameter("postgres-password", Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "sigref", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImage("postgres", "17")
    .WithEnvironment("POSTGRES_DB", Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "postgres")
    .WithBindMount("./config/init-db.sql", "/docker-entrypoint-initdb.d/init-db.sql")
    .WithBindMount("./data/postgres", "/var/lib/postgresql/data")
    .WithArgs("postgres", "-c", "fsync=off");

var hapiDb = postgres.AddDatabase("hapi");
var sigrefDb = postgres.AddDatabase("sigref");

// =============================================================
// KEYCLOAK - Servidor de Autenticación
// =============================================================
//var keycloakHost = Environment.GetEnvironmentVariable("KEYCLOAK_HOST") ?? "keycloak.localhost";
//var keycloakPort = Environment.GetEnvironmentVariable("KEYCLOAK_PORT") ?? "8080";

var keycloak = builder.AddKeycloakWithAutoSetup("keycloak-server", postgres, "keycloak")
    .WithEnvironment("KC_SPI_THEME_LOGIN_THEME", "sigref");

// =============================================================
// HAPI FHIR - Servidor de Datos Clínicos
// =============================================================
var hapi = builder.AddContainer("hapi-fhir", "hapiproject/hapi", "latest")
    .WithEnvironment("SPRING_DATASOURCE_URL", Environment.GetEnvironmentVariable("HAPI_DB_URL") ?? "jdbc:postgresql://postgres:5432/hapi")
    .WithEnvironment("SPRING_DATASOURCE_USERNAME", Environment.GetEnvironmentVariable("HAPI_DB_USER") ?? "hapi")
    .WithEnvironment("SPRING_DATASOURCE_PASSWORD", Environment.GetEnvironmentVariable("HAPI_DB_PASSWORD") ?? "hapi")
    .WithBindMount("./config/hapi.application.yaml", "/app/config/application.yaml")
    .WithHttpEndpoint(targetPort: 8080, port: 8080, name: "hapi-http")
    .WaitFor(postgres)
    .WaitFor(keycloak);

// =============================================================
// SIGREF.API - API Principal del Sistema para Receptoría de Fondos
// =============================================================
var apiHost = Environment.GetEnvironmentVariable("API_HOST") ?? "api.localhost";

var sigrefApi = builder.AddProject<Projects.SIGREF_API>("sigref-api")
    .WithReference(hapiDb)
    .WithReference(sigrefDb)
    .WaitFor(hapi)
    .WaitFor(keycloak);

builder.Build().Run();
