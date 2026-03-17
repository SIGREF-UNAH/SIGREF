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
//   - Claude Sonnet 4.6 (Anthropic AI)     → Resolución de Bug 
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

// WithHostPort expone el puerto al host en TODOS los modos,
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

// ----------------------------------------------------------------------------
// RUTA BASE de la carpeta config/ (relativa al directorio de ejecución).
// AppContext.BaseDirectory es determinista en cualquier entorno (dev, CI/CD).
// ----------------------------------------------------------------------------
var keycloakConfigPath = Path.Combine(AppContext.BaseDirectory, "config");


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

//   Se usa Path.Combine(AppContext.BaseDirectory, ...) para obtener
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
    // MODO DESARROLLO
    // Se usa la imagen oficial con versión fija para evitar pulls inesperados
    // de :latest en cada arranque. Cambiar la versión aquí de forma coordinada
    // con el ARG KEYCLOAK_VERSION del Dockerfile al actualizar.
    //
    //  WithImagePullPolicy(ImagePullPolicy.Missing):
    //   Docker descarga la imagen SOLO si no existe en el daemon local.
    //   Evita el comportamiento por defecto de verificar el registry en cada
    //   arranque cuando se usa una tag mutable. Esto es equivalente a
    //   imagePullPolicy: IfNotPresent en Kubernetes.
    keycloak
        .WithImage("keycloak/keycloak", "26.5.5-0")
        .WithImagePullPolicy(ImagePullPolicy.Missing)

        // Temas: bind-mount para hot-reload en desarrollo sin reconstruir imagen.
        .WithBindMount(
            Path.Combine(keycloakConfigPath, "themes"),
            "/opt/keycloak/themes")

        // --------------------------------------------------------------------
        // REALM — Importación en desarrollo
        // --------------------------------------------------------------------
        // Se monta el mismo JSON que usa el Dockerfile de producción para que
        // el comportamiento sea equivalente en ambos modos.
        //
        // Aspire's AddKeycloak ya incluye internamente "start-dev --import-realm"
        // en su entrypoint. Solo necesitamos montar el archivo en la ruta correcta
        // y Keycloak lo detectará al arrancar.
        //
        // --import-realm es idempotente: importa el realm SOLO si no existe en
        // la BD. Reinicios posteriores no sobreescriben usuarios ni configuración.
        //
        // Para RE-IMPORTAR (reset): borra el volumen de Postgres o elimina el
        // realm manualmente desde la UI de Keycloak Admin.
        // --------------------------------------------------------------------
        .WithBindMount(
            Path.Combine(keycloakConfigPath, "realm-full-export.json"),
            "/opt/keycloak/data/import/sigref-realm.json");
}

// Marca Keycloak como recurso contenedor publicable en el manifiesto de Aspire.
keycloak.PublishAsContainer();

// ============================================================================
// HAPI FHIR — Servidor de Datos Clínicos (HL7 FHIR R4)
// ============================================================================
var hapi = builder.AddHapiFhir("hapifhir")
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword);


// ============================================================================
// ALMACENAMIENTO COMPARTIDO — PDFs e Imágenes
// ============================================================================
const string sharedVolumeName     = "sigref-storage";
const string containerStoragePath = "/app/storage";

var storagePath = Path.GetFullPath(
    Path.Combine(builder.AppHostDirectory, "..", "..", "sigref_storage"));
if (!Directory.Exists(storagePath))
    Directory.CreateDirectory(storagePath);


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
    .WithEnvironment("Hangfire__IsEmbedded", (!useSeparateWorker).ToString())
    .WithEnvironment("ReportStorage__BasePath",
        builder.ExecutionContext.IsRunMode ? storagePath : containerStoragePath)
    .WaitFor(sigrefDb)
    .WaitFor(mongoDb)
    .WaitFor(keycloak)
    .WaitFor(hangfireDb)
    .WithHttpEndpoint(port: 5000, name: "http-dev");

// FIX: instancia propia, no compartida con el Worker
if (builder.ExecutionContext.IsPublishMode)
    sigrefApi.WithAnnotation(new ContainerMountAnnotation(
        source: sharedVolumeName,
        target: containerStoragePath,
        type: ContainerMountType.Volume,
        isReadOnly: false));


// ============================================================================
// HANGFIRE WORKER — Procesador de Tareas Asincrónicas (Opcional / Externo)
// ============================================================================
// Activado solo cuando Hangfire:UseSeparateWorker = true en la configuración.
// Útil en ambientes de alta carga donde se requiere escalar el procesamiento
// de jobs de forma independiente a la API principal.
// ============================================================================

if (useSeparateWorker)
{
    if (builder.ExecutionContext.IsRunMode)
    {
        // RunMode  Aspire lanza el proceso directamente, sin Docker.
        // Storage accedido por ruta física del host (compartida con la API en proceso).
        builder.AddProject<Projects.SIGREF_Hangfire_Worker>("sigref-worker")
            .WithEnvironment("HAPIFHIR_HTTP", hapi.GetEndpoint("http"))
            .WithEnvironment("Hangfire__IsWorkerOnly", "true")
            // FIX: ruta física del host, no la ruta del contenedor
            .WithEnvironment("ReportStorage__BasePath", storagePath)
            .WithReference(sigrefDb)
            .WithReference(hangfireDb)
            .WithReference(keycloak)
            .WaitFor(hangfireDb)
            .WaitFor(sigrefDb)
            .WaitFor(hapi)
            // FIX: WaitFor keycloak que faltaba
            .WaitFor(keycloak);
    }
    else
    {
        // PublishMode construye la imagen con el Dockerfile.
        // Contexto = raíz BE/ para acceder a todos los proyectos de la solución.
        var workerContextDir = Path.GetFullPath(
            Path.Combine(builder.AppHostDirectory, ".."));

        builder.AddDockerfile(
                "sigref-worker",
                workerContextDir,
                "SIGREF.Hangfire.Worker/Worker.Dockerfile")
            .WithEnvironment("HAPIFHIR_HTTP", hapi.GetEndpoint("http"))
            .WithEnvironment("Hangfire__IsWorkerOnly", "true")
            .WithEnvironment("ReportStorage__BasePath", containerStoragePath)
            .WithReference(sigrefDb)
            .WithReference(hangfireDb)
            .WithReference(keycloak)
            .WaitFor(hangfireDb)
            .WaitFor(sigrefDb)
            .WaitFor(hapi)
            // FIX: WaitFor keycloak que faltaba
            .WaitFor(keycloak)
            // FIX: instancia propia de mount, no compartida con la API
            .WithAnnotation(new ContainerMountAnnotation(
                source: sharedVolumeName,
                target: containerStoragePath,
                type: ContainerMountType.Volume,
                isReadOnly: false))
            .PublishAsContainer();
    }
}

// ============================================================================
// FRONTEND — React + Vite
// ----------------------------------------------------------------------------
// RunMode     → Servidor de desarrollo Vite con hot-reload (Node 20 Alpine).
// PublishMode → Imagen de producción construida con Dockerfile y Nginx.
// ============================================================================


IResourceBuilder<ContainerResource>? frontend = null;

if (builder.ExecutionContext.IsRunMode)
{
    frontend = builder.AddContainer("frontend", "node", "20-alpine")
        .WithBindMount("../../FE", "/app")
        .WithEntrypoint("/bin/sh")
        .WithArgs("-c", "cd /app && if [ ! -d 'node_modules' ]; then npm install; fi; npm run dev -- --host 0.0.0.0")

        
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
        // El frontend ahora espera tanto a la API como a
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