using System.Text;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Postgres;

namespace SIGREF.API.AppHost;

/// <summary>
/// Clase para configurar automáticamente el realm de Keycloak en Aspire
/// </summary>
public static class KeycloakRealmConfigurator
{
    /// <summary>
    /// Configura un contenedor de Keycloak con un realm por defecto usando init containers
    /// </summary>
    /// <param name="builder">El builder de la aplicación distribuida</param>
    /// <param name="containerName">Nombre del contenedor de Keycloak</param>
    /// <param name="realmConfigPath">Ruta al archivo de configuración del realm</param>
    /// <returns>El recurso de contenedor de Keycloak configurado</returns>
    public static IResourceBuilder<ContainerResource> AddKeycloakWithRealm(
        this IDistributedApplicationBuilder builder,
        string containerName,
        string? realmConfigPath = null)
    {
        // Usar la configuración por defecto si no se proporciona una ruta
        realmConfigPath ??= "./config/keycloak-realm.json";

        // Configuración básica de Keycloak con argumentos para importar realm
        var keycloak = builder.AddContainer(containerName, "quay.io/keycloak/keycloak", "24.0.3")
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL_HOST", "postgres")
            .WithEnvironment("KC_DB_URL_DATABASE", "keycloak")
            .WithEnvironment("KC_HOSTNAME", "localhost")
            .WithEnvironment("KC_DB_USERNAME", "hapi")
            .WithEnvironment("KC_DB_PASSWORD", "hapi")
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithHttpEndpoint(targetPort: 8080, port: 8081, name: "keycloak-http");

        // Configurar importación del realm si el archivo existe
        if (File.Exists(realmConfigPath))
        {
            keycloak = keycloak
                .WithBindMount(realmConfigPath, "/opt/keycloak/data/import/realm.json")
                .WithArgs("start-dev", "--import-realm");
        }
        else
        {
            keycloak = keycloak.WithArgs("start-dev");
        }

        return keycloak;
    }

    /// <summary>
    /// Configura Keycloak con PostgreSQL y realm por defecto usando init containers
    /// </summary>
    /// <param name="builder">El builder de la aplicación distribuida</param>
    /// <param name="containerName">Nombre del contenedor de Keycloak</param>
    /// <param name="postgresResource">Recurso de PostgreSQL</param>
    /// <param name="databaseName">Nombre de la base de datos para Keycloak</param>
    /// <param name="realmConfigPath">Ruta al archivo de configuración del realm</param>
    /// <returns>El recurso de contenedor de Keycloak configurado</returns>
    public static IResourceBuilder<ContainerResource> AddKeycloakWithPostgresAndRealm(
        this IDistributedApplicationBuilder builder,
        string containerName,
        IResourceBuilder<PostgresServerResource> postgresResource,
        string databaseName = "keycloak",
        string? realmConfigPath = null)
    {
        // Crear la base de datos para Keycloak
        var keycloakDb = postgresResource.AddDatabase(databaseName);
        
        // Usar la configuración por defecto si no se proporciona una ruta
        realmConfigPath ??= "./config/keycloak-realm.json";

        // Configurar Keycloak
        var keycloak = builder.AddContainer(containerName, "quay.io/keycloak/keycloak", "24.0.3")
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL_HOST", "postgres")
            .WithEnvironment("KC_DB_URL_DATABASE", databaseName)
            .WithEnvironment("KC_HOSTNAME", "localhost")
            .WithEnvironment("KC_DB_USERNAME", "hapi")
            .WithEnvironment("KC_DB_PASSWORD", "hapi")
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithHttpEndpoint(targetPort: 8080, port: 8081, name: "keycloak-http")
            .WaitFor(postgresResource);

        // Configurar importación del realm si el archivo existe
        if (File.Exists(realmConfigPath))
        {
            keycloak = keycloak
                .WithBindMount(realmConfigPath, "/opt/keycloak/data/import/realm.json")
                .WithArgs("start-dev", "--import-realm");
        }
        else
        {
            keycloak = keycloak.WithArgs("start-dev");
        }

        return keycloak;
    }

    /// <summary>
    /// Configura Keycloak con setup automático usando un init container
    /// </summary>
    /// <param name="builder">El builder de la aplicación distribuida</param>
    /// <param name="containerName">Nombre del contenedor de Keycloak</param>
    /// <param name="postgresResource">Recurso de PostgreSQL</param>
    /// <param name="databaseName">Nombre de la base de datos para Keycloak</param>
    /// <returns>El recurso de contenedor de Keycloak configurado</returns>
    public static IResourceBuilder<ContainerResource> AddKeycloakWithAutoSetup(
        this IDistributedApplicationBuilder builder,
        string containerName,
        IResourceBuilder<PostgresServerResource> postgresResource,
        string databaseName = "keycloak")
    {
        // Crear la base de datos para Keycloak
        var keycloakDb = postgresResource.AddDatabase(databaseName);

        // Crear el script de inicialización
        CreateKeycloakInitScript();

        // Configurar Keycloak
        var keycloak = builder.AddContainer(containerName, "quay.io/keycloak/keycloak", "24.0.3")
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL_HOST", "postgres")
            .WithEnvironment("KC_DB_URL_DATABASE", databaseName)
            .WithEnvironment("KC_HOSTNAME", "localhost")
            .WithEnvironment("KC_DB_USERNAME", "hapi")
            .WithEnvironment("KC_DB_PASSWORD", "hapi")
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithHttpEndpoint(targetPort: 8080, port: 8081, name: "keycloak-http")
            .WithArgs("start-dev")
            .WaitFor(postgresResource);

        // Agregar init container para configurar el realm
        var keycloakInit = builder.AddContainer("keycloak-init", "curlimages/curl", "latest")
            .WithBindMount("./config/init-keycloak.sh", "/scripts/init-keycloak.sh")
            .WithBindMount("./config/keycloak-realm.json", "/scripts/keycloak-realm.json")
            .WithEntrypoint("/bin/sh")
            .WithArgs("/scripts/init-keycloak.sh")
            .WaitFor(keycloak);

        return keycloak;
    }

    /// <summary>
    /// Crea el script de inicialización de Keycloak
    /// </summary>
    private static void CreateKeycloakInitScript()
    {
        var scriptPath = "./config/init-keycloak.sh";
        
        // Solo crear el archivo si no existe
        if (!File.Exists(scriptPath))
        {
            var directory = Path.GetDirectoryName(scriptPath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // El script ya existe en el sistema de archivos
            // Solo verificamos que esté presente
            Console.WriteLine($"Script de inicialización debe existir en: {Path.GetFullPath(scriptPath)}");
        }
    }

    /// <summary>
    /// Crea la configuración del realm FHIR por defecto
    /// </summary>
    /// <returns>La configuración del realm como JSON</returns>
    public static string CreateDefaultFhirRealmConfig()
    {
        var realmConfig = new
        {
            realm = "fhir",
            displayName = "FHIR Realm",
            enabled = true,
            sslRequired = "external",
            registrationAllowed = true,
            loginWithEmailAllowed = true,
            duplicateEmailsAllowed = false,
            resetPasswordAllowed = true,
            editUsernameAllowed = false,
            bruteForceProtected = true,
            permanentLockout = false,
            maxFailureWaitSeconds = 900,
            minimumQuickLoginWaitSeconds = 60,
            waitIncrementSeconds = 60,
            quickLoginCheckMilliSeconds = 1000,
            maxDeltaTimeSeconds = 43200,
            failureFactor = 30,
            defaultRoles = new[] { "default-roles-fhir", "offline_access", "uma_authorization" },
            requiredCredentials = new[] { "password" },
            passwordPolicy = "hashIterations(27500)",
            clients = new[]
            {
                new
                {
                    clientId = "fhir-client",
                    name = "FHIR Client",
                    description = "Cliente para aplicación FHIR",
                    enabled = true,
                    clientAuthenticatorType = "client-secret",
                    secret = "fhir-client-secret",
                    standardFlowEnabled = true,
                    implicitFlowEnabled = false,
                    directAccessGrantsEnabled = true,
                    serviceAccountsEnabled = true,
                    publicClient = false,
                    protocol = "openid-connect",
                    redirectUris = new[]
                    {
                        "http://localhost:8080/*",
                        "http://localhost:3000/*",
                        "http://localhost:4200/*",
                        "http://127.0.0.1:8080/*",
                        "http://127.0.0.1:3000/*",
                        "http://127.0.0.1:4200/*"
                    },
                    webOrigins = new[]
                    {
                        "http://localhost:8080",
                        "http://localhost:3000",
                        "http://localhost:4200",
                        "http://127.0.0.1:8080",
                        "http://127.0.0.1:3000",
                        "http://127.0.0.1:4200"
                    }
                }
            },
            roles = new
            {
                realm = new[]
                {
                    new
                    {
                        name = "fhir-user",
                        description = "Usuario FHIR estándar",
                        composite = false
                    },
                    new
                    {
                        name = "fhir-admin",
                        description = "Administrador FHIR",
                        composite = false
                    }
                }
            },
            users = new[]
            {
                new
                {
                    username = "fhir-user",
                    enabled = true,
                    emailVerified = true,
                    firstName = "FHIR",
                    lastName = "User",
                    email = "fhir-user@example.com",
                    credentials = new[]
                    {
                        new
                        {
                            type = "password",
                            value = "fhir123",
                            temporary = false
                        }
                    },
                    realmRoles = new[] { "fhir-user" }
                },
                new
                {
                    username = "fhir-admin",
                    enabled = true,
                    emailVerified = true,
                    firstName = "FHIR",
                    lastName = "Admin",
                    email = "fhir-admin@example.com",
                    credentials = new[]
                    {
                        new
                        {
                            type = "password",
                            value = "admin123",
                            temporary = false
                        }
                    },
                    realmRoles = new[] { "fhir-admin" }
                }
            }
        };

        return JsonSerializer.Serialize(realmConfig, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Guarda la configuración del realm por defecto en un archivo
    /// </summary>
    /// <param name="filePath">Ruta donde guardar el archivo</param>
    public static void SaveDefaultRealmConfig(string filePath)
    {
        var realmConfig = CreateDefaultFhirRealmConfig();
        var directory = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, realmConfig, Encoding.UTF8);
    }
}