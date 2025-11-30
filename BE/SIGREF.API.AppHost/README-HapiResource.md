# HAPI FHIR Resource - Aspire Extension

Este archivo contiene la implementación de un recurso personalizado de HAPI FHIR para .NET Aspire.

## Componentes

### `HapiResource`
Clase que representa un servidor HAPI FHIR como un recurso de contenedor en Aspire. Implementa:
- `ContainerResource`: Base para recursos basados en contenedores Docker
- `IResourceWithConnectionString`: Proporciona una cadena de conexión para acceder al servidor FHIR

### `HapiResourceExtensions`
Métodos de extensión para configurar HAPI FHIR en tu aplicación Aspire.

## Uso

### Configuración Básica

```csharp
var hapi = builder
    .AddHapiFhir("hapi-fhir")
    .WithComputeEnvironment(prodEnv);
```

### Con Base de Datos PostgreSQL

```csharp
var hapi = builder
    .AddHapiFhir("hapi-fhir")
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword)
    .WithComputeEnvironment(prodEnv);
```

### Con Archivo de Configuración

```csharp
var hapi = builder
    .AddHapiFhir("hapi-fhir")
    .WithConfigurationFile("./config/hapi.application.yaml")
    .WithComputeEnvironment(prodEnv);
```

### Configuración Completa

```csharp
var hapi = builder
    .AddHapiFhir("hapi-fhir", port: 8080)
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword)
    .WithConfigurationFile("./config/hapi.application.yaml")
    .WithCorsEnabled("*")
    .WithLogLevel("DEBUG")
    .WithComputeEnvironment(prodEnv);
```

## Métodos de Extensión Disponibles

### `AddHapiFhir(name, port?, image?, tag?)`
Agrega un servidor HAPI FHIR a la aplicación.
- **name**: Nombre del recurso
- **port**: Puerto HTTP (opcional)
- **image**: Imagen Docker (default: "hapiproject/hapi")
- **tag**: Tag de la imagen (default: "latest")

### `WithPostgresDatabase(postgresServer, database, username, password)`
Configura la conexión a PostgreSQL para HAPI FHIR y establece la dependencia directa con el contenedor de Postgres para garantizar que ambos servicios compartan red y orden de arranque.

### `WithConfigurationFile(configPath)`
Monta un archivo de configuración YAML en `/app/config/application.yaml`.

### `WithCorsEnabled(allowedOrigins?)`
Habilita CORS con los orígenes permitidos especificados.

### `WithLogLevel(logLevel?)`
Configura el nivel de log de Spring Boot (default: "INFO").

## Beneficios

1. **Tipo Seguro**: `IResourceBuilder<HapiResource>` en lugar de `IResourceBuilder<ContainerResource>`
2. **Intellisense**: Métodos de extensión específicos para HAPI FHIR
3. **Cadena de Conexión**: Acceso fácil a la URL del servidor FHIR
4. **Reutilizable**: Puede ser usado en múltiples proyectos Aspire
5. **Mantenible**: Configuración centralizada y fácil de actualizar

## Ejemplo en AppHost.cs

```csharp
// Configurar HAPI FHIR
var hapi = builder
    .AddHapiFhir("hapi-fhir")
    .WithPostgresDatabase(postgres, hapiDb, postgresUsername, postgresPassword)
    .WithConfigurationFile("./config/hapi.application.yaml")
    .WithComputeEnvironment(prodEnv);

// Referenciar en otros servicios
var sigrefApi = builder
    .AddProject<SIGREF_API>("sigref-api")
    .WaitFor(hapi);

// Agregar al Gateway
yarp.AddRoute("/hapi/{**catch-all}", hapi);
```

## Comparación con Keycloak

Este recurso sigue el mismo patrón que el recurso de Keycloak de Aspire:
- `AddKeycloak()` → `AddHapiFhir()`
- `IResourceBuilder<ContainerResource>` → `IResourceBuilder<HapiResource>`
- Métodos de extensión específicos para el servicio
- Implementación de `IResourceWithConnectionString`

