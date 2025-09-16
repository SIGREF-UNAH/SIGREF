using SIGREF.API.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL con configuración inicial
var username = builder.AddParameter("postgres-username", "hapi");
var password = builder.AddParameter("postgres-password", "hapi", secret: true);

var postgres = builder.AddPostgres("postgres", username, password)
    .WithImage("postgres", "17")
    .WithEnvironment("POSTGRES_DB", "postgres")
    .WithBindMount("./config/init-db.sql", "/docker-entrypoint-initdb.d/init-db.sql")
    .WithHostPort(5432)
    .WithArgs("postgres", "-c", "fsync=off");

// Bases de datos específicas
var hapiDb = postgres.AddDatabase("hapi");

// Keycloak con realm configurado automáticamente usando método más confiable
var keycloak = builder.AddKeycloakWithAutoSetup(
    "keycloak-server", 
    postgres, 
    "keycloak");

// HAPI FHIR
_ = builder.AddContainer("hapi-fhir", "hapiproject/hapi", "latest")
    .WithEnvironment("SPRING_DATASOURCE_URL", "jdbc:postgresql://postgres:5432/hapi")
    .WithEnvironment("SPRING_DATASOURCE_USERNAME", "hapi")
    .WithEnvironment("SPRING_DATASOURCE_PASSWORD", "hapi")
    .WithBindMount("./config/hapi.application.yaml", "/app/config/application.yaml")
    .WithHttpEndpoint(targetPort: 8080, port: 8080, name: "hapi-http")
    .WaitFor(postgres)
    .WaitFor(keycloak);

// Tu API
_ = builder.AddProject<Projects.SIGREF_API>("sigref-api")
    .WithReference(hapiDb);

builder.Build().Run();
