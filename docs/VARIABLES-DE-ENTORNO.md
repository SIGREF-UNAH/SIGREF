# 🔧 Configuración de Variables de Entorno

Este documento describe todas las variables de entorno necesarias para ejecutar SIGREF. Las variables mostradas son **EJEMPLOS Y VALORES POR DEFECTO PARA DESARROLLO**. Debes cambiar la mayoría de ellas según tu entorno y especialmente en producción.

> ⚠️ **IMPORTANTE**: Nunca hagas commit de archivos `.env` con contraseñas o secretos reales. Usa `.env.example` como plantilla.

---

## 📋 Estructura del Archivo `.env`

El archivo `.env` se copia desde `.env.production`:

```bash
cp .env.production .env
```

Luego edita el archivo con tus valores reales:

```bash
# En Linux/Mac
nano .env

# En Windows (PowerShell)
notepad .env
```

---

## 🗄️ Variables de PostgreSQL

Estas variables configuran la conexión a la base de datos relacional principal.

```bash
# ⚠️ CAMBIAR EN TODOS LOS ENTORNOS
POSTGRES_USER=sigref_user                # Usuario de BD (cambiar en producción)
POSTGRES_PASSWORD=your_secure_password   # ❌ CAMBIAR OBLIGATORIAMENTE - Mínimo 16 caracteres
POSTGRES_DB=sigref                       # Nombre de la BD (puedes mantenerlo)

# Opcional - Solo si usas host diferente
POSTGRES_HOST=postgres                   # Hostname (localhost si estás desarrollando)
POSTGRES_PORT=5432                       # Puerto estándar PostgreSQL
```

**Recomendaciones:**
- En **desarrollo**: Puedes usar contraseña simple
- En **producción**: Genera contraseña aleatoria fuerte (mínimo 32 caracteres)
  ```bash
  # Generar contraseña segura en Linux
  openssl rand -base64 32
  ```

---

## 🔐 Variables de Keycloak (Autenticación)

Keycloak es el servidor de identidad y autorización.

```bash
# ⚠️ CAMBIAR EN TODOS LOS ENTORNOS
KEYCLOAK_ADMIN=sigref                    # ❌ CAMBIAR en producción
KEYCLOAK_ADMIN_PASSWORD=sigref           # ❌ CAMBIAR OBLIGATORIAMENTE - Mínimo 16 caracteres
KEYCLOAK_REALM=sigref                    # Realm del sistema (puedes mantenerlo)
KEYCLOAK_FRONTEND_URL=http://localhost:8080  # URL pública de Keycloak

# En producción, usar HTTPS
# KEYCLOAK_FRONTEND_URL=https://auth.tudominio.com
```

**Valores por ambiente:**

### Desarrollo Local
```bash
KEYCLOAK_ADMIN=sigref
KEYCLOAK_ADMIN_PASSWORD=sigref123
KEYCLOAK_FRONTEND_URL=http://localhost:8080
```

### Producción
```bash
KEYCLOAK_ADMIN=admin_user_seguro
KEYCLOAK_ADMIN_PASSWORD=$(openssl rand -base64 32)  # Generar automáticamente
KEYCLOAK_FRONTEND_URL=https://auth.tudominio.com
```

---

## 🔗 Variables de API .NET

Configuración del backend ASP.NET Core.

```bash
# Entorno de ejecución
ASPNETCORE_ENVIRONMENT=Development       # Development, Staging o Production
ASPNETCORE_URLS=http://+:5000            # URL de escucha (puerto 5000)

# JWT - Tokens de autenticación
JWT_AUDIENCE=sigref-api                  # Quién puede usar los tokens (no cambiar)
JWT_ISSUER=http://localhost:8080/realms/sigref  # Quién emite los tokens

# Para producción con HTTPS
# JWT_ISSUER=https://auth.tudominio.com/realms/sigref
# ASPNETCORE_URLS=https://+:5000
# ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/cert.pfx
# ASPNETCORE_Kestrel__Certificates__Default__Password=your_cert_password
```

**Por ambiente:**

### Desarrollo
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
JWT_ISSUER=http://localhost:8080/realms/sigref
```

### Producción
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5000
JWT_ISSUER=https://auth.tudominio.com/realms/sigref
```

---

## 🎨 Variables de Frontend (React)

Configuración del cliente web.

```bash
# ⚠️ Deben coincidir con la configuración del Backend y Keycloak
VITE_API_URL=http://localhost:5000             # URL del API Backend
VITE_KEYCLOAK_URL=http://localhost:8080        # URL de Keycloak
VITE_REALM=sigref                              # Realm (debe coincidir)
VITE_CLIENT_ID=sigref-frontend                 # Client ID en Keycloak

# Opcional - Para desarrollo con diferentes puertos
VITE_DEV_PORT=5173
```

**Por ambiente:**

### Desarrollo Local
```bash
VITE_API_URL=http://localhost:5000
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_REALM=sigref
VITE_CLIENT_ID=sigref-frontend
```

### Producción
```bash
VITE_API_URL=https://api.tudominio.com
VITE_KEYCLOAK_URL=https://auth.tudominio.com
VITE_REALM=sigref
VITE_CLIENT_ID=sigref-frontend
```

---

## 📊 Variables de Logging y Monitoreo

Configuración de logs y observabilidad.

```bash
# Seq - Agregador de logs
SEQ_API_KEY=your_seq_api_key               # ⚠️ Generar en Seq
SEQ_CANONICAL_URI=http://localhost:5341   # URL de Seq
SEQ_ADMIN_PASSWORD=seqadmin123             # ❌ CAMBIAR en producción

# OpenTelemetry - Telemetría
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317

# Aspire Dashboard - Monitoreo
DASHBOARD_CLIENT_SECRET=aspire-dashboard-secret  # ❌ CAMBIAR en producción
DASHBOARD_REQUIRED_ROLE=ti                      # Solo usuarios con rol 'ti'
```

---

## 🏥 Variables de Integración FHIR

```bash
# Servidor FHIR para obtener recursos
FHIR_SERVER_URL=https://hapi.fhir.org/baseR4   # Servidor FHIR público (opcional)

# Si usas HAPI FHIR local
# FHIR_SERVER_URL=http://hapifhir:8080/fhir
```

---

## 💾 Variables de MongoDB

```bash
MONGO_CONNECTION_STRING=mongodb://mongo:27017/sigref_logs
MONGO_DB=sigref_logs
```

---

## 📧 Variables Adicionales (Si aplica)

```bash
# Email para notificaciones
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=tu-email@gmail.com
SMTP_PASSWORD=tu-app-password  # No tu contraseña real
SMTP_FROM=noreply@sigref.hn

# Reportes y documentos
DOCUMENT_STORAGE_PATH=/app/documents
TEMP_FILE_PATH=/app/temp

# Timeouts
API_TIMEOUT=30s
DB_CONNECTION_TIMEOUT=15s
```

---

## 📝 Checklist: Variables Críticas a Cambiar

### 🚨 Cambios Obligatorios ANTES de Deployar a Producción

- [ ] `POSTGRES_PASSWORD` - Contraseña fuerte (32+ caracteres)
- [ ] `KEYCLOAK_ADMIN_PASSWORD` - Contraseña fuerte (32+ caracteres)
- [ ] `SEQ_ADMIN_PASSWORD` - Contraseña fuerte (16+ caracteres)
- [ ] `DASHBOARD_CLIENT_SECRET` - Generar UUID aleatorio
- [ ] `ASPNETCORE_ENVIRONMENT` - Cambiar a `Production`
- [ ] URLs de bases de datos - Apuntar a servidores reales
- [ ] `VITE_API_URL` - Cambiar a dominio de producción
- [ ] `VITE_KEYCLOAK_URL` - Cambiar a dominio de producción
- [ ] `JWT_ISSUER` - Usar URL HTTPS del dominio

---

## 🔄 Ejemplo Completo de `.env` para Producción

```bash
# SERVIDOR DE BASE DE DATOS
POSTGRES_USER=sigref_prod_user
POSTGRES_PASSWORD=GenerarConOpenSSL_Random_32_Characters
POSTGRES_DB=sigref_production
POSTGRES_HOST=db.produccion.ejemplo.com
POSTGRES_PORT=5432

# KEYCLOAK
KEYCLOAK_ADMIN=admin_produccion
KEYCLOAK_ADMIN_PASSWORD=GenerarConOpenSSL_Random_32_Characters
KEYCLOAK_REALM=sigref
KEYCLOAK_FRONTEND_URL=https://auth.tudominio.com

# API .NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5000
JWT_AUDIENCE=sigref-api
JWT_ISSUER=https://auth.tudominio.com/realms/sigref

# FRONTEND REACT
VITE_API_URL=https://api.tudominio.com
VITE_KEYCLOAK_URL=https://auth.tudominio.com
VITE_REALM=sigref
VITE_CLIENT_ID=sigref-frontend

# LOGGING
SEQ_API_KEY=GenerarEnSeq_AdminPanel
SEQ_CANONICAL_URI=https://logs.tudominio.com
SEQ_ADMIN_PASSWORD=GenerarConOpenSSL_Random_16_Characters

# FHIR
FHIR_SERVER_URL=https://hapi.fhir.org/baseR4

# DASHBOARD
DASHBOARD_CLIENT_SECRET=GenerarUUID_Random
DASHBOARD_REQUIRED_ROLE=ti
```

---

## 🛠️ Cómo Generar Contraseñas Seguras

### Linux / Mac / Git Bash
```bash
# Generar 32 caracteres aleatorios base64
openssl rand -base64 32

# Generar UUID
uuidgen

# O usar Python
python3 -c "import secrets; print(secrets.token_urlsafe(32))"
```

### Windows PowerShell
```powershell
# Generar 32 caracteres aleatorios
[Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes(32))

# O usar .NET
[guid]::NewGuid().ToString()
```

---

## 🔒 Secretos Sensibles

**Nunca hagas:**
```bash
# ❌ NO - Nunca commitear con valores reales
git add .env
git commit -m "Add env file"
```

**Usa en su lugar:**
```bash
# ✅ SÍ - Crear template sin valores sensibles
cp .env .env.example
# Editar .env.example para mostrar estructura
git add .env.example
git commit -m "Add env example"

# En producción, pasar variables vía:
# - Docker secrets
# - Environment variables del hosting
# - Vault/Secrets Manager (AWS Secrets, Azure Key Vault, etc.)
```

---

## 🚀 Validación de Variables

Después de configurar `.env`, valida que esté correctamente:

```bash
# Verificar formato
cat .env

# Usar en docker-compose
docker-compose --env-file .env config

# Ver variables en contenedor (desarrollo)
docker-compose exec sigref-api printenv | sort
```

---

## 📚 Referencias

- [Keycloak Configuration](https://www.keycloak.org/server/configuration)
- [PostgreSQL Environment Variables](https://www.postgresql.org/docs/current/app-initdb.html)
- [.NET Configuration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration)
- [Seq Documentation](https://docs.datalust.co/docs/getting-started)

**Última actualización:** 31 de enero de 2026
