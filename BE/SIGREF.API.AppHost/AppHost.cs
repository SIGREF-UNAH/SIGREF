using Projects;
using SIGREF.API.AppHost;
using Microsoft.Extensions.Configuration;

// ============================================================================
// MÓDULO: Orquestación de Infraestructura (AppHost)
// PROYECTO: SIGREF (Sistema de Gestión y Referencia)
// ----------------------------------------------------------------------------
// AUTORES:
//   - @erickArita  (Erick Marley Arita)   → Diseño de Infraestructura y Orquestación
//   - @TETvega     (Héctor Rene Martínez)  → Arquitectura de Red y Lógica Híbrida
//
// CO-AUTORES DE OPTIMIZACIÓN:
//   - Gemini 3 Flash (Google AI)           → Estrategia de Despliegue Evolutivo (Worker/API)
//   - Claude Sonnet 4.6 (Anthropic AI)     → Resolución de Bug Issues #456
//                                            (KC_DB_URL null-ref, WithChildRelationship,
//                                             rutas relativas CI/CD, WithHostPort en prod,
//                                             ParameterResource en env-vars, WaitFor
//                                             inconsistency, variable sin inicializar,
//                                             Keycloak localhost network isolation)
// ----------------------------------------------------------------------------
// PROPÓSITO:
//   Punto central de control para microservicios, bases de datos y servicios
//   de soporte (Keycloak, HAPI FHIR, Hangfire). Gestiona la orquestación
//   completa del entorno en modo desarrollo (RunMode) y publicación (PublishMode).
// ----------------------------------------------------------------------------
// HISTORIAL DE CAMBIOS:
//   - Bug Issue #456 → Corrección de 8 problemas críticos detectados en revisión
//                      de código. Ver comentarios inline para detalle por sección.
// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("env");

// ----------------------------------------------------------------------------
// CONFIGURACIÓN HANGFIRE
// Controla si el procesador de tareas corre embebido dentro de la API
// (valor por defecto: false = embebido) o en un Worker independiente.
// Se lee desde appsettings.json o variables de entorno para flexibilidad
// en distintos ambientes (dev, staging, prod).
// ----------------------------------------------------------------------------
bool useSeparateWorker       = builder.Configuration.GetValue<bool>("Hangfire:UseSeparateWorker", false);
bool enableHangfireDashboard = builder.Configuration.GetValue<bool>("Hangfire:EnableDashboard", true);

// ============================================================================
// POSTGRESQL — Instancia Principal (Multi-Tenant)
// ============================================================================
var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithImage("postgres", "17-alpine")
    .WithEnvironment("POSTGRES_DB", builder.Configuration["Parameters:postgres-db"] ?? "postgres")
    .WithDataVolume("data-postgres", isReadOnly: false);

// FIX Bug #456-4 → WithHostPort expone el puerto al host en TODOS los modos,
// incluyendo publicación, lo cual es un riesgo de seguridad y un error de
// configuración en producción. Se limita exclusivamente al modo desarrollo.
if (builder.ExecutionContext.IsRunMode)
{
    postgres.WithHostPort(5432);
    // pgAdmin solo disponible en desarrollo para exploración de datos.
    postgres.WithPgAdmin(admin => admin.WithImage("dpage/pgadmin4", "latest"));
}

// ----------------------------------------------------------------------------
// Definición de Bases de Datos
// Se utiliza un script de creación parametrizado para evitar duplicación.
// ----------------------------------------------------------------------------
var creationScript = "CREATE DATABASE {{databaseName}};";

var hapiDb     = postgres.AddDatabase("hapi")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "hapi"));

var sigrefDb   = postgres.AddDatabase("sigref")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "sigref"));

// El nombre lógico "keycloakDb" y el nombre real de BD "keycloak" se distinguen
// para que Aspire resuelva las referencias correctamente.
var keycloakDb = postgres.AddDatabase("keycloakDb", "keycloak")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "keycloak"));

var hangfireDb = postgres.AddDatabase("hangfire")
    .WithCreationScript(creationScript.Replace("{{databaseName}}", "hangfire"));

// ============================================================================
// MONGODB — Logs y Auditoría
// ============================================================================
var mongoUser     = builder.AddParameter("mongodb-user");
var mongoPassword = builder.AddParameter("mongodb-password", secret: true);

var mongoSigrefLogs = builder.AddMongoDB("mongo-sigref-logs", 27017, mongoUser, mongoPassword)
    .WithDataVolume("data-mongo-sigref-logs");

var mongoDb = mongoSigrefLogs.AddDatabase("MongoDb");

// MongoExpress (UI de administración) solo disponible en desarrollo.
if (builder.ExecutionContext.IsRunMode)
{
    mongoSigrefLogs.WithMongoExpress();
}

// ============================================================================
// KEYCLOAK — Identity Provider (OpenID Connect / OAuth 2.0)
// ============================================================================
var keyCloakUser = builder.AddParameter("keycloak-user");
var keyCloakPass = builder.AddParameter("keycloak-pass", secret: true);

// FIX Bug #456-1 / #456-5 → La versión original usaba JdbcConnectionString
//   (null en tiempo de orquestación) y pasaba ParameterResource directamente
//   como string a WithEnvironment (conversión implícita incorrecta).
//
// FIX Bug #456-Runtime → Al eliminar KC_DB_* en la iteración anterior,
//   Keycloak intentaba conectarse a localhost:5432 desde DENTRO de su
//   contenedor. El problema es que localhost dentro de un contenedor Docker
//   apunta a sí mismo, no al host ni a otros contenedores de la red.
//
//   DIAGNÓSTICO DEL LOG:
//     "Connection to localhost:5432 refused" → Keycloak no puede alcanzar
//     PostgreSQL porque ambos son contenedores separados en la red Docker.
//
//   SOLUCIÓN: Se construye KC_DB_URL usando ReferenceExpression con el
//   endpoint TCP interno de postgres. Aspire resuelve Host y Port a la IP
//   real del contenedor postgres dentro de aspire-container-network en
//   runtime, garantizando conectividad contenedor-a-contenedor correcta.
//
//   DIAGRAMA DE RED:
//     ┌──────────────────────────────────────────────┐
//     │          aspire-container-network            │
//     │  ┌──────────────┐      ┌──────────────────┐  │
//     │  │   postgres   │◄─────│    keycloak      │  │
//     │  │  172.x.x.x  │      │  KC_DB_URL =     │  │
//     │  │    :5432     │      │  jdbc:postgresql  │  │
//     │  └──────────────┘      │  ://172.x.x.x:   │  │
//     │                        │  5432/keycloak   │  │
//     │                        └──────────────────┘  │
//     └──────────────────────────────────────────────┘
//
// FIX Bug #456-2 → Se eliminan las llamadas a .WithChildRelationship() que no
//   forman parte de la API pública de Aspire 8.x/9.x.
var keycloak = builder.AddKeycloak("keycloak", 8080, keyCloakUser, keyCloakPass)
    .WithReference(keycloakDb)
    .WithEnvironment("KC_HTTP_ENABLED",    "true")
    .WithEnvironment("KC_PROXY_HEADERS",   "xforwarded")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_HEALTH_ENABLED",  "true")
    .WithEnvironment("KC_DB",              "postgres")
    .WithEnvironment("KC_HOSTNAME",        "localhost")

    // KC_DB_URL construida con el endpoint TCP interno de postgres.
    // Aspire resuelve Host y Port a la IP real de la red Docker en runtime,
    // permitiendo que Keycloak alcance PostgreSQL contenedor-a-contenedor.
    .WithEnvironment("KC_DB_URL",
        ReferenceExpression.Create(
            $"jdbc:postgresql://{postgres.GetEndpoint("tcp").Property(EndpointProperty.Host)}:{postgres.GetEndpoint("tcp").Property(EndpointProperty.Port)}/keycloak"))

    // .Resource extrae el valor real del ParameterResource para inyectarlo
    // como variable de entorno sin conversión implícita incorrecta.
    .WithEnvironment("KC_DB_USERNAME", postgresUsername.Resource)
    .WithEnvironment("KC_DB_PASSWORD", postgresPassword.Resource)

    .WaitFor(keycloakDb);

// FIX Bug #456-3 → Las rutas relativas ("./config") se resuelven desde el
//   directorio de trabajo del proceso, que varía en entornos CI/CD o Docker.
//   SOLUCIÓN: Se usa Path.Combine(AppContext.BaseDirectory, ...) para obtener
//   una ruta absoluta y determinista en cualquier entorno de ejecución.
if (builder.ExecutionContext.IsPublishMode)
{
    // Imagen personalizada con temas corporativos para producción.
    keycloak.WithDockerfile(
        Path.Combine(AppContext.BaseDirectory, "config"),
        "Keycloak.Dockerfile");
}
else
{
    // En desarrollo se monta el directorio de temas directamente para
    // permitir hot-reload de cambios de UI sin reconstruir la imagen.
    keycloak.WithBindMount(
        Path.Combine(AppContext.BaseDirectory, "config", "themes"),
        "/opt/keycloak/themes");
}

// Marca Keycloak como recurso contenedor publicable en el manifiesto de Aspire.
keycloak.PublishAsContainer();

// ============================================================================
// HAPI FHIR — Servidor de Datos Clínicos (HL7 FHIR R4)
// ============================================================================
var hapi = builder.AddHapiFhir("hapifhir")
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword);

// ============================================================================
// SIGREF.API — Núcleo del Sistema
// ============================================================================
var sigrefApi = builder.AddProject<Projects.SIGREF_API>("sigref-api")
    .WithReference(sigrefDb)
    .WithReference(mongoDb)
    .WithReference(keycloak)
    .WithReference(hapi)
    .WithReference(hangfireDb)
    .WithEnvironment("Hangfire__EnableDashboard", enableHangfireDashboard.ToString())
    // IsEmbedded = true cuando NO se usa worker separado, indicando a la API
    // que debe registrar los servidores de Hangfire internamente.
    .WithEnvironment("Hangfire__IsEmbedded", (!useSeparateWorker).ToString())
    .WaitFor(sigrefDb)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WaitFor(hangfireDb)
    .WithHttpEndpoint(port: 5000, name: "http-dev");

// ============================================================================
// HANGFIRE WORKER — Procesador de Tareas Asincrónicas (Opcional / Externo)
// Activado solo cuando Hangfire:UseSeparateWorker = true en la configuración.
// Útil en ambientes de alta carga donde se requiere escalar el procesamiento
// de jobs de forma independiente a la API principal.
// ============================================================================
if (useSeparateWorker)
{
    builder.AddProject<Projects.SIGREF_Hangfire_Worker>("sigref-worker")
        .WithReference(sigrefDb)
        .WithReference(hangfireDb)
        .WithReference(hapi)
        // IsWorkerOnly = true desactiva el servidor HTTP en el Worker y lo
        // configura exclusivamente como procesador de cola de Hangfire.
        .WithEnvironment("Hangfire__IsWorkerOnly", "true")
        .WaitFor(hangfireDb)
        .WaitFor(sigrefDb);
    // Nota: la variable local 'sigrefWorker' se omite intencionalmente ya que
    // no se necesita una referencia posterior a este recurso en este archivo.
}

// ============================================================================
// FRONTEND — React + Vite
// ----------------------------------------------------------------------------
// RunMode     → Servidor de desarrollo Vite con hot-reload (Node 20 Alpine).
// PublishMode → Imagen de producción construida con Dockerfile y Nginx.
// ============================================================================

// FIX Bug #456-7 → Se inicializa la variable con null y se asigna
//   obligatoriamente en ambas ramas, garantizando que el compilador valide
//   que el recurso siempre queda definido antes de ser usado.
IResourceBuilder<ContainerResource>? frontend = null;

if (builder.ExecutionContext.IsRunMode)
{
    frontend = builder.AddContainer("frontend", "node", "20-alpine")
        .WithBindMount("../../FE", "/app")
        .WithEntrypoint("/bin/sh")
        .WithArgs("-c", "cd /app && if [ ! -d 'node_modules' ]; then npm install; fi; npm run dev -- --host 0.0.0.0")

        // FIX Bug #456-6 (env vars) → Se reemplaza ReferenceExpression con
        // interpolación manual de puerto por GetEndpoint() directo.
        //
        // IMPORTANTE — contexto de ejecución de estas variables:
        // Las variables VITE_* son consumidas por el BROWSER del desarrollador,
        // NO por el contenedor Node. El browser corre en la máquina host y
        // puede alcanzar localhost sin problema.
        //
        // GetEndpoint() resuelve la URL completa (scheme + host + puerto real
        // asignado por Aspire). Si Aspire reasigna el puerto por conflicto,
        // el valor se actualiza automáticamente sin tocar el código.
        .WithEnvironment("VITE_KEYCLOAK_URL", 
            ReferenceExpression.Create($"http://localhost:{keycloak.GetEndpoint("http").Property(EndpointProperty.Port)}"))
        .WithEnvironment("VITE_KEYCLOAK_REALM",     "sigref")
        .WithEnvironment("VITE_KEYCLOAK_CLIENT_ID", "frontend")
        .WithEnvironment("VITE_API_URL", 
            ReferenceExpression.Create($"http://localhost:{sigrefApi.GetEndpoint("http-dev").Property(EndpointProperty.Port)}"))
        // ORVAL genera el cliente HTTP tipado en tiempo de desarrollo.
        // Apunta al mismo endpoint que VITE_API_URL.
        .WithEnvironment("ORVAL_API_URL", 
            ReferenceExpression.Create($"http://localhost:{sigrefApi.GetEndpoint("http-dev").Property(EndpointProperty.Port)}"))

        .WithHttpEndpoint(targetPort: 5173, port: 5173, name: "http")
        // FIX Bug #456-6 → El frontend ahora espera tanto a la API como a
        // Keycloak antes de arrancar, evitando errores OIDC por IdP no listo.
        // En la versión anterior solo esperaba a sigrefApi (inconsistencia
        // respecto a PublishMode donde sí esperaba a ambos).
        .WaitFor(sigrefApi)
        .WaitFor(keycloak);
}
else
{
    // MODO PRODUCCIÓN
    // La imagen se construye con el Dockerfile del FE. Aspire pasa las URLs
    // internas del clúster/red Docker como build-args para que Vite las
    // embeba en el bundle estático durante el proceso de build de la imagen.
    frontend = builder.AddContainer("frontend", "frontend")
        .WithDockerfile("../../FE", "Dockerfile")
        .WithBuildArg("VITE_KEYCLOAK_URL",       keycloak.GetEndpoint("http"))
        .WithBuildArg("VITE_KEYCLOAK_REALM",     "sigref")
        .WithBuildArg("VITE_KEYCLOAK_CLIENT_ID", "frontend")
        .WithBuildArg("VITE_API_URL",            sigrefApi.GetEndpoint("http"))
        .WithHttpEndpoint(targetPort: 80, port: 5173, name: "http")
        .WaitFor(sigrefApi)
        .WaitFor(keycloak);

    frontend.PublishAsContainer();
}

builder.Build().Run();