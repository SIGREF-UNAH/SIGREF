# 🏗️ Arquitectura Técnica de SIGREF

## Visión General

SIGREF es una plataforma de **3 capas** que implementa una arquitectura moderna y escalable orientada a servicios.

---

## 📐 Capas de la Arquitectura

### 1. **Capa de Presentación (Frontend)**

**Tecnologías:** React 19.1.1 + TypeScript 5.8.3 + Vite 7.1.2

```
┌─────────────────────────────────────────┐
│        NAVEGADOR WEB / CLIENTE           │
├─────────────────────────────────────────┤
│                React App                 │
│  ┌──────────────────────────────────┐   │
│  │ Pages (Páginas/Features)         │   │
│  │ - Patient, Invoice, Dashboard    │   │
│  │ - Cashier, Reports, Admin        │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ Shared Components & Hooks        │   │
│  │ - Forms, Tables, Modals          │   │
│  │ - Custom Hooks (useQuery, etc.)  │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ State Management (Zustand)       │   │
│  │ - Global App State               │   │
│  │ - User Context & Permissions     │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │ API Client (Generado por Orval)  │   │
│  │ - Typed HTTP Requests            │   │
│  │ - Axios + Interceptors           │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
           ▼ HTTP REST
```

### 2. **Capa de Aplicación (Backend API)**

**Tecnologías:** .NET 9.0 + ASP.NET Core + Entity Framework Core

```
┌──────────────────────────────────────────────┐
│        ASPNETCORE - API REST                 │
├──────────────────────────────────────────────┤
│                                              │
│  Controllers (REST Endpoints)               │
│  ├── PatientController                      │
│  ├── InvoiceController                      │
│  ├── CashierController                      │
│  ├── DashboardController                    │
│  ├── AuthController                         │
│  ├── AuditController                        │
│  └── ... más endpoints                      │
│                                              │
│  Middleware & Filters                       │
│  ├── Authentication (JWT/OIDC)              │
│  ├── Authorization (RBAC)                   │
│  ├── Logging & Audit                        │
│  ├── Error Handling                         │
│  └── CORS & Security Headers                │
│                                              │
│  Services (Lógica de Negocio)               │
│  ├── PatientService                        │
│  ├── InvoiceService                        │
│  ├── CashierSessionService                 │
│  ├── DashboardService                      │
│  ├── FhirService                           │
│  ├── KeycloakService                       │
│  ├── AuditService                          │
│  └── ... más servicios                      │
│                                              │
│  Helpers & Utilities                        │
│  ├── FhirUtils (Transformaciones)           │
│  ├── ValidationHelpers                      │
│  ├── PaymentHelpers                         │
│  └── ReportHelpers                          │
│                                              │
└──────────────────────────────────────────────┘
        ▼ SQL / HTTP
```

### 3. **Capa de Datos (Persistencia)**

**Tecnologías:** PostgreSQL 17 + MongoDB 5.0 + Keycloak

```
┌──────────────────────────────────────────────┐
│         DATA & PERSISTENCE LAYER             │
├──────────────────────────────────────────────┤
│                                              │
│  PostgreSQL (Relational Database)           │
│  ├── Hospital Properties                    │
│  ├── Patients (FHIR Resources)             │
│  ├── Practitioners & Organizations          │
│  ├── Health Services & Service Groups       │
│  ├── Invoices & Invoice Items               │
│  ├── Cashier Sessions & Shifts              │
│  ├── Media Files & Documents                │
│  └── Dashboard Facts (Materialized Views)   │
│                                              │
│  MongoDB (Document Database)                │
│  ├── Audit Logs (Inmutable)                │
│  ├── System Logs                            │
│  ├── Error Logs                             │
│  └── User Sessions                          │
│                                              │
│  Keycloak (Identity Provider)               │
│  ├── Usuarios & Roles                       │
│  ├── Permisos & Políticas                   │
│  ├── Sessions & Tokens (JWT)                │
│  └── Configuración OIDC                     │
│                                              │
│  HAPI FHIR Server (Externo)                 │
│  └── Resources FHIR R4/R5 (Sincronización)  │
│                                              │
└──────────────────────────────────────────────┘
```

---

## 🔄 Flujo de una Solicitud (Request Flow)

```
1. Usuario accede a http://localhost
                ↓
2. React carga la SPA (Single Page App)
   - Vite bundlea el código
   - Se cargan assets estáticos
                ↓
3. Frontend intenta acceder a un recurso protegido
   - Se redirige a Keycloak (OIDC)
                ↓
4. Usuario autentica en Keycloak
   - Obtiene Access Token (JWT)
   - Obtiene Refresh Token
                ↓
5. Frontend realiza HTTP request al Backend
   - POST /api/invoices
   - Authorization: Bearer {JWT}
   - Content-Type: application/json
   - Body: { data }
                ↓
6. Nginx (Reverse Proxy) recibe la solicitud
   - Valida certificado SSL (en prod)
   - Redirige a 127.0.0.1:5000 (Backend)
                ↓
7. ASP.NET Core recibe el request
   ├─ Middleware de Autenticación
   │  └─ Valida JWT signature contra Keycloak
   │
   ├─ Middleware de Autorización
   │  └─ Verifica roles en token (RBAC)
   │
   ├─ Middleware de Logging
   │  └─ Registra inicio de request
   │
   └─ Controllers despachador el request
                ↓
8. InvoiceController.CreateInvoice() se ejecuta
   ├─ Validación de entrada (DTOs + Yup)
   ├─ Llamada a InvoiceService.CreateAsync()
   │
   └─ InvoiceService (Lógica de Negocio)
      ├─ Valida reglas de negocio
      ├─ Calcula totales e impuestos
      ├─ Llama a PatientService.GetAsync()
      ├─ Llama a HealthcareFhirService.GetServiceAsync()
      ├─ Persiste en DB vía DbContext
      │  └─ INSERT INTO invoices (...)
      └─ Retorna InvoiceDTO
                ↓
9. Audit Middleware registra la operación
   ├─ Usuario: user@example.com
   ├─ Acción: CREATE_INVOICE
   ├─ Datos: { id, monto, paciente }
   └─ Timestamp: 2026-01-31 14:30:00
   
   MongoDB insert:
   db.audit_logs.insertOne({...})
                ↓
10. Response HTTP 200 OK
    ├─ StatusCode: 200
    ├─ Content-Type: application/json
    └─ Body: { id, createdAt, ... }
                ↓
11. Frontend recibe response
    ├─ TanStack Query cachea el dato
    ├─ Zustand actualiza estado global
    └─ React re-renderiza UI
                ↓
12. Usuario ve la factura creada
```

---

## 🏢 Componentes Principales

### Controllers (Endpoints REST)

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Requiere autenticación
public class InvoicesController : ControllerBase
{
    // GET /api/invoices
    [HttpGet]
    public async Task<IActionResult> GetAll() { }
    
    // GET /api/invoices/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) { }
    
    // POST /api/invoices
    [HttpPost]
    [Authorize(Roles = "cashier,admin")]  // RBAC
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto) { }
    
    // PUT /api/invoices/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceDto dto) { }
    
    // DELETE /api/invoices/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id) { }
}
```

### Services (Lógica de Negocio)

```csharp
public interface IInvoiceService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto);
    Task<InvoiceDto> GetByIdAsync(int id);
    Task<PagedResult<InvoiceDto>> GetAllAsync(int page, int pageSize);
    Task<InvoiceDto> UpdateAsync(int id, UpdateInvoiceDto dto);
    Task DeleteAsync(int id);
    Task<decimal> CalculateTotalAsync(InvoiceDto invoice);
}

public class InvoiceService : IInvoiceService
{
    private readonly SIGREFContext _context;
    private readonly IAuditService _auditService;
    private readonly IPatientService _patientService;
    
    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto)
    {
        // Validar paciente existe
        var patient = await _patientService.GetByIdAsync(dto.PatientId);
        if (patient == null)
            throw new BusinessException("Patient not found");
        
        // Crear factura
        var invoice = new InvoiceEntity
        {
            PatientId = dto.PatientId,
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };
        
        // Guardar en BD
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        
        // Auditar
        await _auditService.LogAsync(new AuditLog
        {
            UserId = GetCurrentUserId(),
            Action = "CREATE_INVOICE",
            EntityId = invoice.Id,
            Changes = JsonSerializer.Serialize(invoice)
        });
        
        return MapToDto(invoice);
    }
}
```

### Entity Framework Core - DbContext

```csharp
public class SIGREFContext : DbContext
{
    public DbSet<InvoiceEntity> Invoices { get; set; }
    public DbSet<PatientEntity> Patients { get; set; }
    public DbSet<CashierSessionEntity> CashierSessions { get; set; }
    public DbSet<AuditLogEntity> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar relaciones
        modelBuilder.Entity<InvoiceEntity>()
            .HasOne(i => i.Patient)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PatientId);
        
        // Índices para performance
        modelBuilder.Entity<InvoiceEntity>()
            .HasIndex(i => i.PatientId);
        
        // Configurar restricciones
        modelBuilder.Entity<InvoiceEntity>()
            .Property(i => i.Amount)
            .HasPrecision(18, 2);
    }
}
```

---

## 🔐 Seguridad en Capas

### Capa 1: Autenticación (OIDC/JWT)

```
Usuario
   ↓ Login
Keycloak
   ↓ OIDC Flow
Frontend obtiene JWT token
   ↓
Almacena en localStorage/sessionStorage
   ↓
Envía en cada request: Authorization: Bearer {JWT}
   ↓
Backend valida JWT signature
   └─ Valida firma contra público key de Keycloak
   └─ Valida expiración
   └─ Valida audience (sigref-api)
```

### Capa 2: Autorización (RBAC)

```
JWT contiene claims:
{
  "sub": "user-id",
  "email": "user@example.com",
  "realm_roles": ["cashier", "admin"],
  "resource_access": {
    "sigref-api": {
      "roles": ["read:invoices", "create:invoices"]
    }
  }
}

[Authorize(Roles = "admin")]
   ↓
Si user.Roles NO contiene "admin"
   ↓
Retorna 403 Forbidden
```

### Capa 3: Auditoría

```
Cada operación sensible es registrada:
- CREATE/UPDATE/DELETE
- Cambios de datos sensibles
- Accesos denegados
- Intentos de login fallidos

Se almacena en MongoDB (inmutable):
{
  "_id": ObjectId(...),
  "userId": "user-123",
  "timestamp": ISODate("2026-01-31T..."),
  "action": "DELETE_INVOICE",
  "entityType": "Invoice",
  "entityId": "inv-456",
  "oldValues": {...},
  "newValues": {...},
  "ipAddress": "192.168.1.100",
  "result": "SUCCESS"
}
```

---

## 📊 Patrón de Datos

### DTOs (Data Transfer Objects)

Para transferencias seguras y validadas:

```csharp
// Frontend → Backend
public record CreateInvoiceDto(
    int PatientId,
    int ServiceId,
    decimal Amount,
    string PaymentMethod
);

// Backend → Frontend
public record InvoiceDto(
    int Id,
    int PatientId,
    string PatientName,
    decimal Amount,
    DateTime CreatedAt,
    InvoiceStatusEnum Status
);
```

### Mapeo Automático

Usar AutoMapper para convertir Entities ↔ DTOs:

```csharp
// Configuración
CreateMap<InvoiceEntity, InvoiceDto>()
    .ForMember(d => d.PatientName, 
        opt => opt.MapFrom(s => s.Patient.FullName));

// Uso
var dto = _mapper.Map<InvoiceDto>(entity);
```

---

## ⚡ Performance Patterns

### 1. Lazy Loading & Pagination

```csharp
// ❌ MAL - Carga todos los registros
var all = _context.Invoices.ToList();

// ✅ BIEN - Paginación
var page = await _context.Invoices
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 2. Proyección en Queries

```csharp
// ❌ MAL - Carga entidad completa
var invoices = await _context.Invoices
    .Include(i => i.Patient)
    .ToListAsync();

// ✅ BIEN - Solo campos necesarios
var invoices = await _context.Invoices
    .Select(i => new InvoiceListDto
    {
        Id = i.Id,
        Amount = i.Amount,
        PatientName = i.Patient.FullName
    })
    .ToListAsync();
```

### 3. Caching

```csharp
// IMemoryCache para datos estáticos
var services = _cache.Get<List<HealthService>>("health_services");
if (services == null)
{
    services = await _context.HealthServices.ToListAsync();
    _cache.Set("health_services", services, TimeSpan.FromHours(1));
}
```

---

## 🔌 Integración FHIR

```
┌─────────────────────────────────────────┐
│      SIGREF Backend (.NET)              │
│  FhirService                            │
│  └─ FhirClient (Hl7.Fhir library)      │
└──────────────┬──────────────────────────┘
               │
        HTTP REST calls
               │
    ┌──────────▼──────────┐
    │  HAPI FHIR Server   │
    │  https://hapi...    │
    │                     │
    │  Resources:         │
    │  - Patient          │
    │  - Practitioner     │
    │  - Organization     │
    │  - Location         │
    │  - HealthcareService│
    └─────────────────────┘
```


## 🐳 Despliegue en Docker

Cada componente en su propio contenedor:

```yaml
services:
  frontend:
    image: sigref-frontend:latest
    ports: ["80:80"]
    depends_on: [nginx]
    
  backend:
    image: sigref-api:latest
    ports: ["5000:5000"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on: [postgres, keycloak, mongo]
    
  postgres:
    image: postgres:17-alpine
    volumes: [pgdata:/var/lib/postgresql/data]
    
  mongodb:
    image: mongo:5.0
    volumes: [mongodata:/data/db]
    
  keycloak:
    image: keycloak/keycloak:latest
    environment:
      - KEYCLOAK_ADMIN=sigref
      - KEYCLOAK_ADMIN_PASSWORD=...
    
  nginx:
    image: nginx:alpine
    ports: ["80:80", "443:443"]
    volumes: [./nginx.conf:/etc/nginx/nginx.conf:ro]
```

---

**Última actualización:** 31 de enero de 2026
