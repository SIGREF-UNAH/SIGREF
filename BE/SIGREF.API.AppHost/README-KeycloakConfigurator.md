# Configurador de Realm de Keycloak para Aspire

Esta clase proporciona múltiples métodos para configurar automáticamente Keycloak con un realm por defecto en aplicaciones .NET Aspire.

## Características

- **Múltiples estrategias** de configuración de realm
- **Configuración automática** de Keycloak con PostgreSQL
- **Importación confiable** del realm FHIR
- **Init containers** para setup post-arranque
- **Métodos de extensión** para fácil integración

## Métodos Disponibles

### 1. `AddKeycloakWithPostgresAndRealm` (Método Simple)
Configuración usando el flag `--import-realm` de Keycloak.

```csharp
var keycloak = builder.AddKeycloakWithPostgresAndRealm(
    "keycloak-server", 
    postgres, 
    "keycloak", 
    "./config/keycloak-realm.json");
```

**Pros:** Simple, directo
**Contras:** A veces no funciona en todos los entornos

### 2. `AddKeycloakWithAutoSetup` (Método Recomendado)
Configuración usando init container con API Admin de Keycloak.

```csharp
var keycloak = builder.AddKeycloakWithAutoSetup(
    "keycloak-server", 
    postgres, 
    "keycloak");
```

**Pros:** 
- ✅ Más confiable
- ✅ Verifica si el realm ya existe
- ✅ Usa API Admin de Keycloak
- ✅ Mejor manejo de errores

**Contras:** 
- Requiere script adicional
- Más complejo

### 3. `AddKeycloakWithRealm` (Método Básico)
Para configuraciones sin PostgreSQL.

```csharp
var keycloak = builder.AddKeycloakWithRealm(
    "keycloak-server", 
    "./config/keycloak-realm.json");
```

## Configuración Recomendada

Para la máxima confiabilidad, usa el método `AddKeycloakWithAutoSetup`:

```csharp
using SIGREF.API.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Configurar PostgreSQL
var postgres = builder.AddPostgres("postgres", "hapi", "hapi");

// Configurar Keycloak con setup automático (RECOMENDADO)
var keycloak = builder.AddKeycloakWithAutoSetup(
    "keycloak-server", 
    postgres, 
    "keycloak");
```

## Cómo Funciona el Setup Automático

1. **Keycloak se inicia** normalmente
2. **Init container** espera a que Keycloak esté disponible
3. **Obtiene token** de acceso usando admin/admin
4. **Verifica** si el realm 'fhir' ya existe
5. **Crea el realm** usando la API Admin si no existe
6. **Maneja errores** y proporciona logs detallados

## Archivos Necesarios

Para que funcione correctamente, necesitas estos archivos:

```
SIGREF.API.AppHost/config/
├── keycloak-realm.json       # Configuración del realm FHIR
├── init-keycloak.sh         # Script de inicialización
├── init-db.sql             # Script de BD
└── hapi.application.yaml   # Config HAPI FHIR
```

## Configuración del Realm FHIR

El realm incluye:

### Clientes
- **fhir-client**: Cliente principal (secret: fhir-client-secret)
- **fhir-admin**: Cliente administrativo (secret: fhir-admin-secret)

### Roles
- **fhir-user**: Usuario estándar de FHIR
- **fhir-admin**: Administrador de FHIR

### Usuarios de Prueba
- **fhir-user** / **fhir123**: Usuario estándar
- **fhir-admin** / **admin123**: Usuario administrador

## Troubleshooting

### Si el realm no se crea:

1. **Verifica los logs** del init container:
   ```bash
   docker logs <keycloak-init-container-id>
   ```

2. **Verifica que Keycloak esté funcionando**:
   ```bash
   curl http://localhost:8081/health/ready
   ```

3. **Verifica el archivo de realm**:
   ```bash
   cat ./config/keycloak-realm.json
   ```

### Si prefieres el método simple:

Cambia a `AddKeycloakWithPostgresAndRealm` en tu `AppHost.cs`, pero ten en cuenta que puede ser menos confiable.

## Acceso a Keycloak

Una vez configurado:

- **URL**: http://localhost:8081
- **Admin**: admin / admin
- **Realm**: fhir
- **Admin Console**: http://localhost:8081/admin

## Beneficios de la Nueva Implementación

1. **Confiabilidad**: Usa API Admin en lugar de importación automática
2. **Verificación**: Comprueba si el realm ya existe
3. **Logs**: Proporciona información detallada del proceso
4. **Flexibilidad**: Múltiples métodos según necesidades
5. **Robustez**: Manejo de errores y reintentos