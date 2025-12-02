# Guía de Despliegue SIGREF con Cloudflare Tunnel

## Requisitos Previos

1. **Cloudflare Tunnel instalado** (`cloudflared`)
2. **Docker y Docker Compose**
3. **Dominio configurado en Cloudflare**

## Pasos de Instalación

### 1. Descargar OpenTelemetry Java Agent para HAPI FHIR

Nota: La imagen oficial de HAPI FHIR ya incluye el agente OpenTelemetry, solo necesitas activarlo con variables de entorno.

### 2. Crear archivo de contraseñas para nginx

```bash
# Instalar htpasswd (si no está instalado)
sudo apt-get update
sudo apt-get install apache2-utils -y

# Crear directorio nginx si no existe
mkdir -p nginx

# Crear archivo .htpasswd
htpasswd -c nginx/.htpasswd admin

# Se te pedirá ingresar una contraseña para el usuario 'admin'
```

### 3. Configurar variables de entorno

```bash
# Copiar ejemplo de producción
cp .env.production .env

# Editar el archivo .env con tus valores
nano .env
```

**⚠️ IMPORTANTE: Cambiar estos valores:**

- `POSTGRES_PASSWORD` - Contraseña segura para PostgreSQL
- `MONGODB_PASSWORD` - Contraseña segura para MongoDB
- `KEYCLOAK_PASSWORD` - Contraseña segura para admin de Keycloak
- `KEYCLOAK_HOSTNAME` - Tu dominio (ejemplo: sigref.unah.edu.hn)
- `HAPI_SERVER_ADDRESS` - URL pública de HAPI (https://tu-dominio.com/fhir)
- `HAPI_OAUTH_ISSUER` - URL del realm de Keycloak (https://tu-dominio.com/auth/realms/sigref)
- `HAPI_OAUTH_CLIENT_SECRET` - Secreto del cliente (configurar después en Keycloak)
- `FRONTEND_KEYCLOAK_URL` - URL pública de Keycloak (https://tu-dominio.com/auth)
- `FRONTEND_API_URL` - URL pública de la API (https://tu-dominio.com/api)

### 4. Configurar Cloudflare Tunnel

```bash
# Instalar cloudflared
wget -q https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
sudo dpkg -i cloudflared-linux-amd64.deb

# Autenticar con Cloudflare
cloudflared tunnel login

# Crear tunnel
cloudflared tunnel create sigref

# El comando anterior imprimirá un TUNNEL-ID, copiarlo

# Crear archivo de configuración
mkdir -p ~/.cloudflared
nano ~/.cloudflared/config.yml
```

**Contenido de `config.yml`:**

```yaml
tunnel: <TUNNEL-ID>
credentials-file: /home/<TU-USUARIO>/.cloudflared/<TUNNEL-ID>.json

ingress:
  - hostname: tu-dominio.com
    service: http://localhost:80
  - service: http_status:404
```

```bash
# Crear ruta DNS
cloudflared tunnel route dns sigref tu-dominio.com

# Instalar como servicio systemd
sudo cloudflared service install
sudo systemctl start cloudflared
sudo systemctl enable cloudflared

# Verificar estado
sudo systemctl status cloudflared
```

### 5. Iniciar servicios

```bash
# Construir e iniciar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Verificar estado
docker-compose ps
```

### 6. Configurar Keycloak (Primera vez)

1. **Acceder al admin console**: `https://tu-dominio.com/auth/admin`
2. **Login** con usuario `admin` y la contraseña configurada en `.env`
3. **Crear realm** llamado `sigref`:
   - Click en el dropdown superior izquierdo donde dice "Master"
   - Click "Create Realm"
   - Name: `sigref`
   - Enabled: `On`
   - Click "Create"

4. **Crear cliente para HAPI FHIR**:
   - Ir a Clients → Create client
   - Client type: `OpenID Connect`
   - Client ID: `hapi-fhir`
   - Click "Next"
   - Client authentication: `On`
   - Authorization: `Off`
   - Authentication flow: marcar `Standard flow`, `Direct access grants`
   - Click "Next"
   - Root URL: `https://tu-dominio.com/fhir`
   - Home URL: `https://tu-dominio.com/fhir`
   - Valid redirect URIs: `https://tu-dominio.com/fhir/*`
   - Web origins: `https://tu-dominio.com`
   - Click "Save"
   - Ir a la pestaña "Credentials"
   - Copiar el `Client secret` y agregarlo a `.env` como `HAPI_OAUTH_CLIENT_SECRET`

5. **Crear cliente para Frontend**:
   - Ir a Clients → Create client
   - Client type: `OpenID Connect`
   - Client ID: `frontend`
   - Click "Next"
   - Client authentication: `Off` (public client)
   - Authentication flow: marcar `Standard flow`
   - Click "Next"
   - Root URL: `https://tu-dominio.com`
   - Home URL: `https://tu-dominio.com`
   - Valid redirect URIs: `https://tu-dominio.com/*`
   - Web origins: `https://tu-dominio.com`
   - Click "Save"

6. **Reiniciar servicios** después de configurar el client secret:
   ```bash
   docker-compose restart hapifhir sigref-api frontend
   ```

### 7. Verificar OpenTelemetry en Aspire Dashboard

1. Acceder a `https://tu-dominio.com/dashboard/`
2. Ingresar credenciales de nginx (usuario: `admin`, contraseña: la que configuraste con htpasswd)
3. Verificar que aparezcan los servicios:
   - ✅ `sigref-api`
   - ✅ `hapi-fhir`
   - ✅ `keycloak`

## URLs de Acceso

| Servicio | URL | Autenticación |
|----------|-----|---------------|
| **Frontend** | https://tu-dominio.com/ | Keycloak OIDC |
| **API** | https://tu-dominio.com/api/ | JWT Bearer |
| **Keycloak** | https://tu-dominio.com/auth/ | Admin console |
| **Aspire Dashboard** | https://tu-dominio.com/dashboard/ | Keycloak OIDC (rol: ti) |
| **Seq (Logs)** | http://servidor:5341 | Admin local / OIDC opcional |
| **Health Check** | https://tu-dominio.com/health | Público |

> **Nota**: HAPI FHIR no está expuesto públicamente. Solo es accesible internamente via la API.

## Configuración de Seq (Persistencia de Logs)

### Primer Inicio

1. **Acceder a Seq**: `http://servidor:5341`
2. **Login**: Usuario `admin`, contraseña de `SEQ_ADMIN_PASSWORD` en `.env`
3. **Crear API Key para el Collector**:
   - Ir a Settings → API Keys → Add API Key
   - Title: `otel-collector`
   - Minimum level: `Verbose` (para recibir todos los niveles)
   - Permissions: `Ingest`
   - Click "Save"
   - Copiar la API Key generada
   - Agregar a `.env` como `SEQ_API_KEY=<la-key>`
4. **Reiniciar el collector**:
   ```bash
   docker-compose restart otel-collector
   ```

### Configurar Autenticación OIDC con Keycloak (Opcional)

1. En Seq: Settings → System → Authentication → Change
2. Seleccionar "OpenID Connect"
3. Configurar:
   - Authority: `https://tu-dominio.com/auth/realms/sigref`
   - Client ID: `seq`
   - Client Secret: (obtener de Keycloak)
4. En Keycloak, crear cliente `seq`:
   - Client authentication: On
   - Valid redirect URIs: `http://servidor:5341/*`
5. Guardar y reiniciar Seq

## Documentación de Referencia

### Keycloak OpenTelemetry
- **Documentación oficial**: https://www.keycloak.org/keycloak-benchmark/kubernetes-guide/latest/customizing-deployment#KC_OTEL
- Configuración usada:
  - `KC_FEATURES: "preview"` - Habilita características experimentales
  - `KC_METRICS_ENABLED: "true"` - Habilita métricas
  - Variables OTEL estándar para exportar a Aspire

### HAPI FHIR OpenTelemetry
- **Documentación oficial**: https://github.com/hapifhir/hapi-fhir-jpaserver-starter#enable-opentelemetry-auto-instrumentation
- La imagen de HAPI incluye el agente Java OpenTelemetry
- Se activa con: `JAVA_TOOL_OPTIONS: "-javaagent:/app/opentelemetry-javaagent.jar"`
- Compatible con exportadores OTLP (Aspire Dashboard)

## Comandos Útiles

```bash
# Ver logs de un servicio específico
docker-compose logs -f sigref-api
docker-compose logs -f hapifhir
docker-compose logs -f keycloak

# Reiniciar un servicio
docker-compose restart keycloak

# Reconstruir y reiniciar
docker-compose up -d --build

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (¡CUIDADO! Borra datos)
docker-compose down -v

# Ver estado de Cloudflare Tunnel
sudo systemctl status cloudflared
sudo journalctl -u cloudflared -f
```

## Troubleshooting

### HAPI FHIR no envía telemetría

**Verificar que el agente OpenTelemetry está disponible:**
```bash
docker-compose exec hapifhir ls -l /app/opentelemetry-javaagent.jar
```

**Ver logs de HAPI:**
```bash
docker-compose logs -f hapifhir | grep -i otel
```

La imagen oficial de HAPI ya incluye el agente, solo asegúrate de que `JAVA_TOOL_OPTIONS` esté configurado.

### Keycloak no envía telemetría

**Verificar features habilitadas:**
```bash
docker-compose logs keycloak | grep -i "preview\|metrics\|otel"
```

Keycloak requiere `KC_FEATURES: "preview"` para habilitar OpenTelemetry.

### No puedo acceder al dashboard

**Verificar archivo .htpasswd:**
```bash
ls -l nginx/.htpasswd
cat nginx/.htpasswd
```

**Recrear contraseña:**
```bash
htpasswd -c nginx/.htpasswd admin
docker-compose restart nginx
```

### Cloudflare Tunnel no funciona

**Ver logs:**
```bash
sudo journalctl -u cloudflared -f
```

**Reiniciar servicio:**
```bash
sudo systemctl restart cloudflared
```

**Verificar configuración:**
```bash
cat ~/.cloudflared/config.yml
cloudflared tunnel info sigref
```

### Error de conexión a bases de datos

**Verificar que las bases existan:**
```bash
docker-compose exec postgres psql -U sigref_user -l
```

Deberías ver: `keycloak`, `hapi`, `sigref`, `postgres`

**Si faltan, recrear contenedor de postgres:**
```bash
docker-compose down postgres
docker volume rm sigref_postgres-data
docker-compose up -d postgres
```

## Seguridad

### Checklist de Seguridad

- ✅ Todas las contraseñas en `.env` son seguras (mínimo 16 caracteres)
- ✅ Dashboard protegido con autenticación básica
- ✅ No hay puertos expuestos públicamente (solo a través de Cloudflare)
- ✅ Cloudflare maneja SSL/TLS automáticamente
- ✅ Headers de seguridad configurados en nginx
- ✅ CORS configurado correctamente en todos los servicios
- ✅ OAuth configurado para HAPI FHIR

### Headers de Seguridad Configurados

```nginx
X-Frame-Options: SAMEORIGIN
X-Content-Type-Options: nosniff
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

## Mantenimiento

### Backup de PostgreSQL

```bash
# Backup completo
docker-compose exec postgres pg_dumpall -U sigref_user > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup de una base específica
docker-compose exec postgres pg_dump -U sigref_user sigref > sigref_backup_$(date +%Y%m%d).sql
```

### Backup de MongoDB

```bash
# Backup
docker-compose exec mongodb mongodump \
  --username mongo_user \
  --password "$MONGODB_PASSWORD" \
  --authenticationDatabase admin \
  --out /backup

# Copiar backup al host
docker cp $(docker-compose ps -q mongodb):/backup ./mongodb_backup_$(date +%Y%m%d)
```

### Restaurar desde Backup

**PostgreSQL:**
```bash
cat backup_20250101_120000.sql | docker-compose exec -T postgres psql -U sigref_user
```

**MongoDB:**
```bash
docker cp ./mongodb_backup_20250101 $(docker-compose ps -q mongodb):/restore
docker-compose exec mongodb mongorestore \
  --username mongo_user \
  --password "$MONGODB_PASSWORD" \
  --authenticationDatabase admin \
  /restore
```

### Actualizar Servicios

```bash
# Pull latest images
docker-compose pull

# Rebuild y restart
docker-compose up -d --build

# Ver logs durante actualización
docker-compose logs -f
```

### Monitoreo

**Ver uso de recursos:**
```bash
docker stats
```

**Ver logs en tiempo real:**
```bash
# Todos los servicios
docker-compose logs -f

# Servicio específico
docker-compose logs -f sigref-api

# Últimas 100 líneas
docker-compose logs --tail=100 hapifhir
```

## Arquitectura del Sistema

```
Internet
    ↓
Cloudflare (CDN + DDoS Protection + SSL)
    ↓
Cloudflare Tunnel (cloudflared)
    ↓
Nginx Reverse Proxy (Puerto 80)
    ├── / → Frontend (React)
    ├── /api/ → SIGREF API (.NET)
    ├── /auth/ → Keycloak (Identity)
    └── /dashboard/ → Aspire Dashboard (Keycloak OIDC)
```

### Red Interna de Docker

```
sigref-network (bridge)
  ├── nginx:80 ─────────────────┐
  ├── frontend:80               │
  ├── sigref-api:8080 ──────────┼──→ otel-collector:4317
  ├── keycloak:8080 ────────────┼──→ otel-collector:4317
  ├── hapifhir:8080 ────────────┼──→ otel-collector:4317
  ├── otel-collector:4317 ──────┼──→ env-dashboard:18889 (tiempo real)
  │                             └──→ seq:80 (persistencia)
  ├── env-dashboard:18888 (UI) + 18889 (OTLP)
  ├── seq:80 (interno) + 5341 (externo)
  ├── postgres:5432
  └── mongodb:27017
```

### Flujo de Telemetría

```
Servicios (.NET, Java, Keycloak)
    │
    ▼ OTLP gRPC
OpenTelemetry Collector (:4317)
    │
    ├──→ Aspire Dashboard (:18889) - Visualización en tiempo real
    │
    └──→ Seq (:80) - Persistencia y búsqueda de logs
```

## Preguntas Frecuentes

**P: ¿Por qué usar Cloudflare Tunnel en lugar de exponer puertos?**  
R: Cloudflare Tunnel proporciona conexión segura sin abrir puertos del firewall, protección DDoS gratuita, SSL automático, y caché CDN.

**P: ¿Puedo usar un subdominio diferente para cada servicio?**  
R: Sí, en Cloudflare Tunnel puedes configurar múltiples hostnames. Ejemplo:
```yaml
ingress:
  - hostname: app.tu-dominio.com
    service: http://localhost:80
  - hostname: api.tu-dominio.com
    service: http://localhost:80
```

**P: ¿Cómo agrego más usuarios al dashboard?**  
R: Usa `htpasswd` sin `-c` para agregar usuarios adicionales:
```bash
htpasswd nginx/.htpasswd otro_usuario
```

**P: ¿Los servicios pueden comunicarse entre sí?**  
R: Sí, todos están en la red `sigref-network` y pueden usar los nombres de servicio como hostname (ejemplo: `http://keycloak:8080`).

## Soporte

- **Documentación HAPI FHIR**: https://hapifhir.io/hapi-fhir/docs/
- **Documentación Keycloak**: https://www.keycloak.org/documentation
- **Cloudflare Tunnel Docs**: https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/
- **Aspire Dashboard**: https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard

---

**¡Listo!** Tu sistema SIGREF está desplegado de forma segura con Cloudflare Tunnel y monitoreo completo con OpenTelemetry.
