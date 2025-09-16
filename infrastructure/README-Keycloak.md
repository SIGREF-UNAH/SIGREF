# 🔐 Configuración de Keycloak para FHIR

Este directorio contiene la configuración automática de Keycloak para trabajar con el servidor FHIR HAPI.

## 🚀 Inicio Rápido

Para levantar todo el stack con Keycloak preconfigurado:

```bash
docker compose up -d --build
```

El proceso de inicialización configurará automáticamente:
- ✅ Realm FHIR
- ✅ Clientes OAuth2/OIDC
- ✅ Usuarios de prueba
- ✅ Roles y permisos

## 📋 Servicios Incluidos

| Servicio | Puerto | URL | Descripción |
|----------|--------|-----|-------------|
| **PostgreSQL** | 5432 | `localhost:5432` | Base de datos para HAPI FHIR y Keycloak |
| **HAPI FHIR** | 8080 | `http://localhost:8080` | Servidor FHIR |
| **Keycloak** | 8081 | `http://localhost:8081` | Servidor de autenticación |

## 🔑 Credenciales de Acceso

### Administrador de Keycloak
- **URL**: http://localhost:8081/admin
- **Usuario**: `admin`
- **Contraseña**: `admin`

### Base de Datos PostgreSQL
- **Host**: `localhost:5432`
- **Usuario**: `hapi`
- **Contraseña**: `hapi`
- **Bases de datos**:
  - `hapi` (para FHIR)
  - `keycloak` (para Keycloak)

## 👥 Usuarios de Prueba (Realm FHIR)

| Usuario | Contraseña | Rol | Descripción |
|---------|------------|-----|-------------|
| `admin` | `admin123` | `fhir-admin` | Administrador del sistema |
| `practitioner` | `practitioner123` | `fhir-practitioner` | Profesional de la salud |
| `patient` | `patient123` | `fhir-patient` | Paciente |

## 🔧 Clientes OAuth2/OIDC Configurados

### Cliente FHIR (`fhir-client`)
- **Client ID**: `fhir-client`
- **Client Secret**: `fhir-client-secret`
- **Grant Types**: Authorization Code, Client Credentials, Direct Grant
- **Redirect URIs**: 
  - `http://localhost:8080/*`
  - `http://localhost:3000/*`
  - `http://localhost:4200/*`

### Cliente Admin (`fhir-admin`)
- **Client ID**: `fhir-admin`
- **Client Secret**: `fhir-admin-secret`
- **Grant Types**: Authorization Code, Client Credentials, Direct Grant
- **Uso**: Operaciones administrativas

## 🌐 Endpoints Importantes

### Realm FHIR
- **Base URL**: `http://localhost:8081/realms/fhir`

### Endpoints OAuth2/OIDC
```
# Obtener token
POST http://localhost:8081/realms/fhir/protocol/openid-connect/token

# Información del usuario
GET http://localhost:8081/realms/fhir/protocol/openid-connect/userinfo

# Cerrar sesión
GET http://localhost:8081/realms/fhir/protocol/openid-connect/logout
```

## 🧪 Ejemplo de Obtención de Token

### Usando curl (Client Credentials)
```bash
curl -X POST http://localhost:8081/realms/fhir/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=fhir-client" \
  -d "client_secret=fhir-client-secret"
```

### Usando curl (Password Grant)
```bash
curl -X POST http://localhost:8081/realms/fhir/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=fhir-client" \
  -d "client_secret=fhir-client-secret" \
  -d "username=practitioner" \
  -d "password=practitioner123"
```

## 🔄 Reconfiguración

Si necesitas reconfigurar Keycloak desde cero:

```bash
# Detener servicios
docker compose down

# Eliminar volúmenes (¡CUIDADO! Esto borra todos los datos)
docker volume rm happyfhir_db_data

# Reiniciar
docker compose up -d --build
```

## 📁 Archivos de Configuración

- `keycloak-realm.json`: Configuración completa del realm FHIR
- `init-keycloak.sh`: Script de inicialización automática
- `Dockerfile.keycloak-init`: Imagen Docker para la inicialización
- `create-keycloak-db.sql`: Script de creación de bases de datos

## 🔍 Verificación de Estado

Para verificar que Keycloak está funcionando correctamente:

```bash
# Verificar salud de Keycloak
curl http://localhost:8081/health

# Verificar realm FHIR
curl http://localhost:8081/realms/fhir

# Ver logs de inicialización
docker logs keycloak-init
```

## 🛠️ Troubleshooting

### Keycloak no inicia
1. Verificar que PostgreSQL esté funcionando: `docker logs db`
2. Verificar logs de Keycloak: `docker logs keycloak`
3. Asegurar que el puerto 8081 no esté en uso

### La inicialización falla
1. Verificar logs del inicializador: `docker logs keycloak-init`
2. Verificar conectividad: `docker exec keycloak-init curl http://keycloak:8080/health`
3. Reiniciar el servicio de inicialización: `docker compose restart keycloak-init`

### Problemas de autenticación
1. Verificar que el realm `fhir` existe en http://localhost:8081/admin
2. Verificar configuración de clientes
3. Verificar usuarios y contraseñas

## 🔐 Seguridad en Producción

⚠️ **IMPORTANTE**: Esta configuración es para desarrollo. Para producción:

1. Cambiar todas las contraseñas por defecto
2. Usar HTTPS (SSL/TLS)
3. Configurar `KC_HOSTNAME` correctamente
4. Deshabilitar `start-dev` y usar `start`
5. Configurar certificados SSL
6. Revisar configuración de CORS y redirect URIs
7. Configurar backup de la base de datos

## 📚 Referencias

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [HAPI FHIR Documentation](https://hapifhir.io/hapi-fhir/docs/)
- [OAuth 2.0 / OIDC with FHIR](https://www.hl7.org/fhir/security.html)