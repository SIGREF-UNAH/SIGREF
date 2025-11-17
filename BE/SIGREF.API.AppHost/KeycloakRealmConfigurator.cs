using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Postgres;

namespace SIGREF.API.AppHost;

public static class KeycloakRealmConfigurator
{
    private const string DefaultRealmPath = "./config/keycloak-realm.json";
    private const string DefaultKeycloakImage = "quay.io/keycloak/keycloak";
    private const string DefaultKeycloakVersion = "24.0.3";

    public static IResourceBuilder<ContainerResource> AddKeycloakWithAutoSetup(
        this IDistributedApplicationBuilder builder,
        string containerName,
        IResourceBuilder<PostgresServerResource>? postgresResource = null,
        string databaseName = "keycloak",
        string? realmConfigPath = null)
    {
        realmConfigPath ??= DefaultRealmPath;

        EnsureRealmConfig(realmConfigPath);

        postgresResource ??= builder.Resources.OfType<IResourceBuilder<PostgresServerResource>>().FirstOrDefault()
            ?? throw new InvalidOperationException("No se encontró un recurso PostgreSQL en la aplicación.");

        var keycloakDb = postgresResource.AddDatabase(databaseName);

        var dbUser = Environment.GetEnvironmentVariable("KEYCLOAK_DB_USER") ?? "sigref";
        var dbPass = Environment.GetEnvironmentVariable("KEYCLOAK_DB_PASS") ?? "sigref";
        var adminUser = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN") ?? "admin";
        var adminPass = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASS") ?? "admin";
        var hostname = Environment.GetEnvironmentVariable("KEYCLOAK_HOSTNAME") ?? "localhost";

        var keycloak = CreateBaseKeycloakContainer(builder, containerName, databaseName, dbUser, dbPass, adminUser, adminPass, hostname)
            .WithBindMount(realmConfigPath, "/opt/keycloak/data/import/realm.json")
            .WithArgs("start-dev", "--import-realm")
            .WaitFor(postgresResource);

        Console.WriteLine("[SIGREF] Keycloak se inicializará automáticamente con --import-realm.");
        Console.WriteLine($"[SIGREF] Realm generado dinámicamente desde variables .env → {realmConfigPath}");

        return keycloak;
    }

    private static IResourceBuilder<ContainerResource> CreateBaseKeycloakContainer(
        IDistributedApplicationBuilder builder,
        string containerName,
        string dbName,
        string dbUser,
        string dbPass,
        string adminUser,
        string adminPass,
        string hostname)
    {
        return builder.AddContainer(containerName, DefaultKeycloakImage, DefaultKeycloakVersion)
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL_HOST", "postgres")
            .WithEnvironment("KC_DB_URL_DATABASE", dbName)
            .WithEnvironment("KC_DB_USERNAME", dbUser)
            .WithEnvironment("KC_DB_PASSWORD", dbPass)
            .WithEnvironment("KC_HOSTNAME", hostname)
            .WithEnvironment("KEYCLOAK_ADMIN", adminUser)
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", adminPass)
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithEnvironment("KC_SPI_THEME_LOGIN_THEME", "sigref")
            .WithEnvironment("KC_SPI_THEME_ACCOUNT_THEME", "keycloak")
            .WithEnvironment("KC_THEME_CACHE_TEMPLATES", "false")
            .WithEnvironment("KC_THEME_CACHE_THEMES", "false")
            .WithBindMount("./config/themes", "/opt/keycloak/themes")
            .WithHttpEndpoint(targetPort: 8080, port: 8081, name: "keycloak-http");
    }

    private static void EnsureRealmConfig(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[SIGREF] No se encontró {filePath}. Generando realm con roles definidos en .env...");
            Console.ResetColor();
            SaveDefaultRealmConfig(filePath);
        }
        else
        {
            Console.WriteLine($"[SIGREF] Realm existente encontrado en {filePath}.");
        }
    }

    public static string CreateDefaultRealmConfig()
    {
        var realmName = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? "sigref";
        var realmDisplay = Environment.GetEnvironmentVariable("KEYCLOAK_REALM_DISPLAY") ?? "SIGREF Realm";

        var rolesEnv = Environment.GetEnvironmentVariable("KEYCLOAK_ROLES");
        var roleNames = string.IsNullOrWhiteSpace(rolesEnv)
            ? Array.Empty<string>()
            : rolesEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // === Usuarios iniciales ===
        var adminUser = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_USERNAME") ?? "sigref-admin";
        var adminPass = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? "Admin123";
        var adminEmail = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_EMAIL") ?? "admin@sigref.local";
        var adminFirst = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_FIRSTNAME") ?? "SIGREF";
        var adminLast = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_LASTNAME") ?? "Admin";

        // === Cliente Backend (API principal) ===
        var apiClient = new
        {
            clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_API_ID") ?? "sigref-api",
            name = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_API_NAME") ?? "SIGREF_API",
            description = "Cliente OIDC para la API de SIGREF",
            enabled = true,
            clientAuthenticatorType = "client-secret",
            secret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_API_SECRET") ?? "sigref-secret",
            protocol = "openid-connect",
            standardFlowEnabled = true,
            directAccessGrantsEnabled = true,
            publicClient = false,
            redirectUris = (Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_API_REDIRECT_URIS") ?? "https://api.localhost/*")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            webOrigins = (Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_API_WEB_ORIGINS") ?? "https://api.localhost")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        };

        // === Cliente Frontend (público) ===
        var feClient = new
        {
            clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_FE_ID") ?? "frontend",
            name = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_FE_NAME") ?? "SIGREF_Frontend",
            description = "Aplicación web del SIGREF",
            enabled = true,
            publicClient = true,
            protocol = "openid-connect",
            standardFlowEnabled = true,
            directAccessGrantsEnabled = true,
            redirectUris = (Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_FE_REDIRECT_URIS") ?? "http://localhost:5173/*")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            webOrigins = (Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_FE_WEB_ORIGINS") ?? "http://localhost:5173")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        };

        // === Cliente de administración interna ===
        var adminApiClient = new
        {
            clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ADMIN_ID") ?? "sigref-admin-api",
            name = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ADMIN_NAME") ?? "SIGREF Admin API",
            description = "Cliente interno usado por SIGREF.API para crear y administrar usuarios",
            enabled = true,
            protocol = "openid-connect",
            serviceAccountsEnabled = true,
            clientAuthenticatorType = "client-secret",
            secret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ADMIN_SECRET") ?? "sigref-admin-secret",
            publicClient = false,
            standardFlowEnabled = false,
            directAccessGrantsEnabled = false,
            authorizationServicesEnabled = true,
            fullScopeAllowed = false
        };

        var realmRoles = roleNames
            .Select(r => new { name = r, description = $"Rol definido en entorno: {r}", composite = false })
            .ToArray();

        var realmConfig = new
        {
            realm = realmName,
            displayName = realmDisplay,
            loginTheme = "sigref",
            enabled = true,
            sslRequired = "external",
            registrationAllowed = false,
            loginWithEmailAllowed = true,
            resetPasswordAllowed = true,
            bruteForceProtected = true,

            // --- Clientes registrados ---
            clients = new object[] { apiClient, feClient, adminApiClient },

            // --- Roles ---
            roles = new { realm = realmRoles },

            // --- Usuario administrador inicial ---
            users = new[]
            {
                new {
                    username = adminUser,
                    enabled = true,
                    emailVerified = true,
                    firstName = adminFirst,
                    lastName = adminLast,
                    email = adminEmail,
                    credentials = new[] {
                        new { type = "password", value = adminPass, temporary = false }
                    },
                    realmRoles = roleNames
                }
            }
        };

        return JsonSerializer.Serialize(realmConfig, new JsonSerializerOptions { WriteIndented = true });
    }

    public static void SaveDefaultRealmConfig(string filePath)
    {
        var content = CreateDefaultRealmConfig();
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    private static string GetFileHash(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        var hash = sha.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }
}
