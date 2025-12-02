# Sistema de Auditoría (Logs) - SIGREF API

## Descripción

Sistema de auditoría automático basado en el estándar FHIR AuditEvent que registra todas las operaciones CRUD en MongoDB.

## Características

✅ **Registro automático** de operaciones:
- **CREATE** (POST) - Guarda los datos creados
- **UPDATE** (PUT/PATCH) - Guarda los datos modificados
- **DELETE** - Registra la eliminación
- **READ** (GET) - Solo cuando se consulta un recurso específico (con ID)

✅ **Información capturada**:
- Usuario que realizó la acción (ID, nombre, roles)
- Tipo de recurso FHIR afectado
- ID del recurso
- Timestamp (UTC)
- Endpoint y método HTTP
- IP del cliente
- Código de estado HTTP
- Datos antes/después del cambio
- Éxito/fallo de la operación

✅ **Almacenamiento en MongoDB**:
- Base de datos: `sigref-logs`
- Colección: `audit_logs`
- Índices optimizados para consultas rápidas

## Uso

### Automático

El sistema funciona automáticamente mediante un middleware. No necesitas hacer nada en tus controladores.

### Consultar logs

Endpoints disponibles (requieren rol `admin` o `auditor`):

```http
# Obtener un log específico por ID
GET /api/Audit/{id}
Ejemplo: GET /api/Audit/507f1f77bcf86cd799439011

# Obtener todos los logs (con paginación)
GET /api/Audit?page=1&pageSize=50
Parámetros:
  - page: número de página (default: 1)
  - pageSize: cantidad de registros por página (default: 50, máximo: 100)

# Obtener logs de un recurso específico
GET /api/Audit/resource/{resourceType}/{resourceId}
Ejemplo: GET /api/Audit/resource/Patient/123

# Obtener logs de un usuario
GET /api/Audit/user/{userId}?from=2024-01-01&to=2024-12-31

# Obtener logs por tipo de acción
GET /api/Audit/action/{action}?from=2024-01-01&to=2024-12-31
Acciones: create, update, delete, read
```

### Consultar directamente en MongoDB

```javascript
// Ver todos los logs
db.audit_logs.find().sort({ timestamp: -1 }).limit(10)

// Logs de un paciente específico
db.audit_logs.find({ 
  resourceType: "Patient", 
  resourceId: "123" 
})

// Logs de un usuario
db.audit_logs.find({ userId: "user-id" })

// Logs de creaciones
db.audit_logs.find({ action: "create" })

// Logs con errores
db.audit_logs.find({ success: false })
```

## Estructura del Log

```json
{
  "_id": "ObjectId",
  "action": "create|update|delete|read",
  "resourceType": "Patient",
  "resourceId": "123",
  "userId": "user-id",
  "userName": "john.doe",
  "userRoles": ["admin", "cashier"],
  "timestamp": "2024-12-02T10:30:00Z",
  "endpoint": "/api/Patient/123",
  "httpMethod": "POST",
  "statusCode": 201,
  "clientIp": "192.168.1.100",
  "dataBefore": null,
  "dataAfter": "{...}",
  "additionalInfo": {},
  "success": true,
  "errorMessage": null
}
```

## Configuración

El sistema está configurado en `Startup.cs`:

```csharp
// Servicios
services.AddAuditServices();

// Middleware (debe ir después de UseRouting y antes de UseAuthentication)
app.UseAuditMiddleware();
```

## Recursos FHIR Detectados

El sistema detecta automáticamente estos tipos de recursos:
- Patient
- Practitioner
- PractitionerRole
- Organization
- Location
- HealthcareService
- Encounter
- Observation
- Condition
- Procedure
- MedicationRequest
- Appointment
- ServiceRequest
- DiagnosticReport
- Invoice
- Account

## Exclusiones

No se auditan:
- Endpoints de Swagger (`/swagger`)
- Health checks (`/health`)
- Archivos estáticos (`/media`)
- Listados completos (GET sin ID específico)

## Rendimiento

- Los logs se guardan de forma **asíncrona** sin bloquear las peticiones
- Índices en MongoDB para consultas rápidas
- El middleware tiene un impacto mínimo en el rendimiento

## Seguridad

- Solo usuarios con roles `admin` o `auditor` pueden consultar los logs
- Los logs son inmutables (solo inserción, no modificación)
- Se registra la IP del cliente para trazabilidad
