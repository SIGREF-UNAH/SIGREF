# 📘 Manual Técnico - SIGREF
## Sistema de Gestión de la Receptoría de Fondos

**Versión**: 1.0.0  
**Fecha**: Diciembre 2024  
**Equipo de Desarrollo**: Ver sección [Equipo](#equipo-de-desarrollo)

---

## 📑 Tabla de Contenidos

1. [Introducción](#1-introducción)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Stack Tecnológico](#3-stack-tecnológico)
4. [Estructura del Proyecto](#4-estructura-del-proyecto)
5. [Backend - API](#5-backend---api)
6. [Frontend - Web](#6-frontend---web)
7. [Base de Datos](#7-base-de-datos)
8. [Autenticación y Autorización](#8-autenticación-y-autorización)
9. [Integración FHIR](#9-integración-fhir)
10. [Observabilidad y Monitoreo](#10-observabilidad-y-monitoreo)
11. [Despliegue](#11-despliegue)
12. [Guía de Desarrollo](#12-guía-de-desarrollo)
13. [API Reference](#13-api-reference)
14. [Troubleshooting](#14-troubleshooting)

---

## 1. Introducción

### 1.1 Propósito del Documento

Este manual técnico proporciona documentación completa para desarrolladores, arquitectos y personal técnico que trabaja con el sistema SIGREF. Incluye:

- Arquitectura y diseño del sistema
- Documentación de código
- Guías de desarrollo
- Procedimientos de despliegue
- Referencias de API

### 1.2 Audiencia

- **Desarrolladores**: Implementación de nuevas funcionalidades
- **Arquitectos**: Diseño y evolución del sistema
- **DevOps**: Despliegue y mantenimiento
- **QA**: Pruebas y validación

### 1.3 Alcance

SIGREF es un sistema web full-stack que gestiona:

- Ingresos y facturación de servicios médicos
- Información de pacientes (FHIR-compliant)
- Catálogo de servicios y paquetes
- Gestión de empleados y roles
- Sesiones de caja y turnos
- Reportes administrativos
- Auditoría completa del sistema

---

## 2. Arquitectura del Sistema

### 2.1 Arquitectura General

```
┌─────────────────────────────────────────────────────────────┐
│                      CLOUDFLARE CDN                         │
│                  (SSL, DDoS Protection)                     │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                 CLOUDFLARE TUNNEL                           │
│                  (cloudflared)                              │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                   NGINX (Reverse Proxy)                     │
│  ┌──────────┬──────────┬──────────┬──────────────────────┐ │
│  │ Frontend │ API      │ Keycloak │ Aspire Dashboard     │ │
│  │ (React)  │ (.NET 8) │ (Auth)   │ (Monitoring)         │ │
│  └──────────┴──────────┴──────────┴──────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
┌───────▼──────┐  ┌─────▼──────┐  ┌─────▼──────┐
│  PostgreSQL  │  │  MongoDB   │  │ HAPI FHIR  │
│  (Relational)│  │  (Logs)    │  │ (R4 Server)│
└──────────────┘  └────────────┘  └────────────┘
```

### 2.2 Patrón de Arquitectura

**Arquitectura en Capas (Layered Architecture)**

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (React Components, Pages, Routers)     │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Application Layer               │
│  (Controllers, API Endpoints)           │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Business Logic Layer            │
│  (Services, Domain Logic)               │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Data Access Layer               │
│  (Repositories, FHIR Client, EF Core)   │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Data Layer                      │
│  (PostgreSQL, MongoDB, HAPI FHIR)       │
└─────────────────────────────────────────┘
```

### 2.3 Principios de Diseño


**Separation of Concerns (SoC)**
- Cada capa tiene responsabilidades bien definidas
- Frontend y Backend completamente desacoplados
- Servicios independientes y reutilizables

**Dependency Injection (DI)**
- Inyección de dependencias en .NET
- Facilita testing y mantenibilidad
- Reduce acoplamiento entre componentes

**RESTful API Design**
- Endpoints siguiendo convenciones REST
- Uso apropiado de verbos HTTP
- Códigos de estado HTTP semánticos

**FHIR-Like Architecture**
- Recursos basados en estándares HL7 FHIR R5
- Interoperabilidad con sistemas de salud
- Estructura de datos estandarizada

---

## 3. Stack Tecnológico

### 3.1 Frontend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **React** | 19.1.1 | Framework UI principal |
| **TypeScript** | 5.8.3 | Lenguaje tipado |
| **Vite** | 7.1.2 | Build tool y dev server |
| **Tailwind CSS** | 4.1.13 | Framework CSS |
| **Ant Design** | 5.27.4 | Componentes UI |
| **TanStack Query** | 5.90.2 | Data fetching y caching |
| **Zustand** | 5.0.8 | State management |
| **React Router** | 7.9.1 | Routing |
| **Axios** | 1.12.2 | HTTP client |
| **Formik** | 2.4.6 | Form management |
| **Yup** | 1.7.0 | Schema validation |
| **Orval** | 7.13.1 | API client generator |
| **Keycloak JS** | 26.2.0 | Authentication client |
| **CASL** | 6.7.3 | Authorization |

### 3.2 Backend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 9.0 | Framework principal |
| **ASP.NET Core** | 9.0 | Web API framework |
| **Entity Framework Core** | 9.0.9 | ORM para PostgreSQL |
| **HAPI FHIR** | 5.12.2 | Cliente FHIR R4 |
| **Npgsql** | 9.0.4 | PostgreSQL provider |
| **MongoDB Driver** | 3.5.0 | Cliente MongoDB |
| **Aspire** | 13.0.1 | Orchestration y observability |
| **OpenTelemetry** | Latest | Telemetría distribuida |

### 3.3 Infraestructura

| Servicio | Versión | Propósito |
|----------|---------|-----------|
| **PostgreSQL** | 17-alpine | Base de datos relacional |
| **MongoDB** | Latest | Base de datos de logs |
| **Keycloak** | Latest | Identity & Access Management |
| **HAPI FHIR Server** | Latest | Servidor FHIR R4 |
| **Nginx** | Alpine | Reverse proxy |
| **Seq** | Latest | Log aggregation |
| **Aspire Dashboard** | Latest | Monitoring dashboard |
| **Docker** | Latest | Containerization |
| **Cloudflare Tunnel** | Latest | Secure access |

---

## 4. Estructura del Proyecto

### 4.1 Estructura General

```
SIGREF/
├── BE/                          # Backend
│   ├── SIGREF.API/             # API principal
│   └── SIGREF.API.ServiceDefaults/  # Configuración Aspire
├── FE/                          # Frontend
│   ├── src/                    # Código fuente
│   ├── public/                 # Assets estáticos
│   └── documentation/          # Documentación
├── config/                      # Configuraciones
│   ├── init-databases.sql      # Script de inicialización DB
│   └── otel-collector-config.yaml  # Config OpenTelemetry
├── nginx/                       # Configuración Nginx
│   └── sites/default.conf      # Virtual host config
├── docker-compose.yml          # Orquestación de servicios
├── .env.production             # Variables de entorno
├── MANUAL-DE-USUARIO.md        # Manual de usuario
├── MANUAL-TECNICO.md           # Este documento
└── README.md                   # Documentación general
```

### 4.2 Estructura del Backend

```
BE/SIGREF.API/
├── Audit/                      # Sistema de auditoría
│   ├── Extensions/            # Extensiones de auditoría
│   ├── Middleware/            # Middleware de auditoría
│   ├── Models/                # Modelos de auditoría
│   └── Services/              # Servicios de auditoría
├── Constants/                  # Constantes del sistema
│   ├── Env.cs                 # Variables de entorno
│   ├── PaymentMethods.cs      # Métodos de pago
│   ├── RolesConstants.cs      # Roles del sistema
│   └── TerminologyConstants.cs # Terminología FHIR
├── Controllers/                # Controladores API
│   ├── Audit/                 # Endpoints de auditoría
│   ├── Auth/                  # Endpoints de autenticación
│   ├── Cashier/               # Endpoints de caja
│   ├── Healthcare/            # Endpoints de servicios
│   ├── Invoices/              # Endpoints de facturación
│   ├── Location/              # Endpoints de ubicaciones
│   ├── Organization/          # Endpoints de organizaciones
│   ├── Patient/               # Endpoints de pacientes
│   └── Practitioner/          # Endpoints de empleados
├── Database/                   # Capa de datos
│   ├── Configurations/        # Configuraciones EF Core
│   ├── Entity/                # Entidades del modelo
│   │   ├── Administration/    # Entidades administrativas
│   │   ├── Billing/           # Entidades de facturación
│   │   ├── Cashier/           # Entidades de caja
│   │   ├── Catalogs/          # Catálogos
│   │   └── Files/             # Archivos multimedia
│   ├── Migrations/            # Migraciones EF Core
│   ├── Seeding/               # Datos iniciales
│   ├── SIGREFContext.cs       # DbContext principal
│   └── SIGREFSeeder.cs        # Seeder principal
├── Dtos/                       # Data Transfer Objects
│   ├── Administration/        # DTOs administrativos
│   ├── Auth/                  # DTOs de autenticación
│   ├── Cashier/               # DTOs de caja
│   ├── Common/                # DTOs comunes
│   ├── Healthcare/            # DTOs de servicios
│   ├── Invoice/               # DTOs de facturación
│   ├── Location/              # DTOs de ubicaciones
│   ├── Organization/          # DTOs de organizaciones
│   ├── Patient/               # DTOs de pacientes
│   └── Practitioner/          # DTOs de empleados
├── Extensions/                 # Métodos de extensión
│   ├── Common/                # Extensiones comunes
│   ├── CashierSessionExtensions.cs
│   ├── HealthcareExtensions.cs
│   ├── LocationExtensions.cs
│   ├── OrganizationExtensions.cs
│   ├── PatientExtensions.cs
│   └── PractitionerExtensions.cs
├── Helpers/                    # Clases auxiliares
│   ├── EnumSchemaFilter.cs    # Filtro Swagger para enums
│   ├── MediaFilesHelper.cs    # Helper para archivos
│   ├── PaginationHelper.cs    # Helper de paginación
│   └── ResponseHelper.cs      # Helper de respuestas
├── Services/                   # Lógica de negocio
│   ├── AdministrationHospital/ # Servicios administrativos
│   ├── Auth/                  # Servicios de autenticación
│   ├── Billing/               # Servicios de facturación
│   ├── Cashier/               # Servicios de caja
│   ├── Common/                # Servicios comunes
│   ├── Files/                 # Servicios de archivos
│   ├── Healthcare/            # Servicios médicos
│   ├── Location/              # Servicios de ubicaciones
│   ├── Organization/          # Servicios de organizaciones
│   ├── Patient/               # Servicios de pacientes
│   └── Practitioner/          # Servicios de empleados
├── media/                      # Archivos multimedia
├── Program.cs                  # Punto de entrada
├── Startup.cs                  # Configuración de servicios
├── appsettings.json           # Configuración
└── SIGREF.API.csproj          # Archivo de proyecto
```

### 4.3 Estructura del Frontend

```
FE/
├── src/
│   ├── api/                    # Cliente API generado
│   │   ├── models/            # Modelos TypeScript
│   │   ├── mutator/           # Configuración Axios
│   │   ├── healthcares/       # Hooks de servicios
│   │   ├── locations/         # Hooks de ubicaciones
│   │   ├── organizations/     # Hooks de organizaciones
│   │   ├── patients/          # Hooks de pacientes
│   │   └── practitioner/      # Hooks de empleados
│   ├── auth/                   # Autenticación
│   │   ├── abilities.ts       # Definición de permisos CASL
│   │   ├── keycloak.ts        # Configuración Keycloak
│   │   └── roles.ts           # Roles del sistema
│   ├── config/                 # Configuración
│   │   ├── providers/         # Providers de contexto
│   │   ├── index.ts           # Config principal
│   │   ├── routes.ts          # Definición de rutas
│   │   └── shortcuts.ts       # Atajos de teclado
│   ├── features/               # Módulos funcionales
│   │   ├── events/            # Módulo de eventos
│   │   ├── healthcares/       # Módulo de servicios
│   │   ├── incomes/           # Módulo de ingresos
│   │   ├── locations/         # Módulo de ubicaciones
│   │   ├── organizations/     # Módulo de organizaciones
│   │   ├── patients/          # Módulo de pacientes
│   │   ├── practitioners/     # Módulo de empleados
│   │   ├── reports/           # Módulo de reportes
│   │   └── service-groups/    # Módulo de paquetes
│   ├── routers/                # Configuración de rutas
│   │   ├── AppRouter.tsx      # Router principal
│   │   └── index.ts           # Exportaciones
│   ├── shared/                 # Recursos compartidos
│   │   ├── components/        # Componentes reutilizables
│   │   │   ├── modals/        # Componentes modales
│   │   │   └── ui/            # Componentes UI
│   │   ├── constants/         # Constantes
│   │   ├── hooks/             # Hooks personalizados
│   │   ├── pages/             # Páginas reutilizables
│   │   ├── store/             # Estado global (Zustand)
│   │   └── utils/             # Utilidades
│   ├── App.tsx                 # Componente raíz
│   ├── main.tsx                # Punto de entrada
│   └── index.css               # Estilos globales
├── documentation/              # Documentación
│   ├── components/            # Docs de componentes
│   ├── config/                # Docs de configuración
│   ├── hooks/                 # Docs de hooks
│   └── pages/                 # Docs de páginas
├── public/                     # Assets estáticos
├── .env                        # Variables de entorno
├── orval.config.cjs           # Configuración Orval
├── package.json               # Dependencias
├── tsconfig.json              # Configuración TypeScript
├── vite.config.ts             # Configuración Vite
└── tailwind.config.js         # Configuración Tailwind
```

---

## 5. Backend - API

### 5.1 Arquitectura del Backend

El backend sigue una arquitectura en capas con los siguientes componentes:

**Capas Principales:**

1. **Controllers**: Endpoints HTTP, validación de entrada
2. **Services**: Lógica de negocio
3. **Repositories**: Acceso a datos (EF Core, FHIR Client)
4. **DTOs**: Transferencia de datos
5. **Extensions**: Mapeo entre DTOs y entidades

### 5.2 Configuración de Servicios

**Program.cs** - Punto de entrada y configuración de Aspire:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Aspire Service Defaults (OpenTelemetry, Health Checks)
builder.AddServiceDefaults();

// PostgreSQL con integración Aspire
builder.AddNpgsqlDbContext<SIGREFContext>("sigref");

// MongoDB para logs
builder.AddMongoDBClient("sigref-logs");

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services, builder);

var app = builder.Build();
app.MapDefaultEndpoints();

startup.Configure(app, app.Environment);

// Aplicar migraciones automáticamente
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<SIGREFContext>();
if (context.Database.GetPendingMigrations().Any())
{
    context.Database.Migrate();
}

app.Run();
```

**Startup.cs** - Configuración de servicios y middleware:

```csharp
public void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
{
    // Registrar servicios
    services.AddScoped<FhirService>();
    services.AddScoped<FhirClient>(sp => 
        sp.GetRequiredService<FhirService>().GetFhirClient());
    
    // Servicios de negocio
    services.AddScoped<IPatientService, PatientService>();
    services.AddScoped<IPractitionerService, PractitionerService>();
    services.AddScoped<IOrganizationService, OrganizationService>();
    services.AddScoped<LocationService>();
    services.AddScoped<HealthcareService>();
    services.AddScoped<ServiceGroupService>();
    services.AddScoped<IShiftService, ShiftService>();
    services.AddScoped<ICashierSessionService, CashierSessionService>();
    services.AddScoped<IInvoiceService, InvoiceService>();
    
    // Servicios de autenticación
    services.AddScoped<IKeycloakClient, KeycloakClient>();
    services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
    services.AddScoped<IUserContextService, UserContextService>();
    
    // Auditoría
    services.AddAuditServices();
    
    // Autenticación JWT con Keycloak
    services.AddAuthentication()
        .AddKeycloakJwtBearer(
            serviceName: "keycloak",
            realm: _configuration["Keycloak:RealmName"],
            options => {
                options.Audience = _configuration["Keycloak:Audience"];
                options.Authority = _configuration["Keycloak:Authority"];
                // Configuración de roles...
            });
    
    // CORS
    services.AddCors(opt => {
        var allowURLS = _configuration.GetSection("AllowURLS").Get<string[]>();
        opt.AddPolicy("CorsPolicy", builder => builder
            .WithOrigins(allowURLS)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    });
}
```

### 5.3 Patrón de Controladores

Todos los controladores siguen un patrón consistente:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET: api/patients
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFiltered([FromQuery] PatientFilterDto filter)
    {
        var result = await _patientService.GetFilteredPatientsAsync(filter);
        return Ok(result);
    }

    // GET: api/patients/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
            return NotFound($"Patient with id '{id}' not found.");
        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var created = await _patientService.CreatePatientAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/patients/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(string id, [FromBody] UpdatePatientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var updated = await _patientService.UpdatePatientAsync(id, dto);
        return Ok(updated);
    }

    // DELETE: api/patients/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(string id)
    {
        await _patientService.DeletePatientAsync(id);
        return NoContent();
    }
}
```

### 5.4 Patrón de Servicios

Los servicios implementan la lógica de negocio:

```csharp
public interface IPatientService
{
    Task<PatientDto> CreatePatientAsync(CreatePatientDto dto);
    Task<PatientDto> GetPatientByIdAsync(string id);
    Task<PatientDto> UpdatePatientAsync(string id, UpdatePatientDto dto);
    Task DeletePatientAsync(string id);
    Task<PagedResult<PatientDto>> GetFilteredPatientsAsync(PatientFilterDto filter);
}

public class PatientService : IPatientService
{
    private readonly FhirClient _fhirClient;
    private const string ResourceType = "Patient";

    public PatientService(FhirClient fhirClient)
    {
        _fhirClient = fhirClient ?? throw new ArgumentNullException(nameof(fhirClient));
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto)
    {
        // 1. Convertir DTO a recurso FHIR
        var patient = dto.ToFhirPatient();
        
        // 2. Crear en servidor FHIR
        var created = await _fhirClient.CreateAsync(patient);
        
        // 3. Convertir respuesta a DTO
        return created.ToDto();
    }

    public async Task<PatientDto> GetPatientByIdAsync(string id)
    {
        var patient = await _fhirClient.ReadAsync<FhirPatient>($"{ResourceType}/{id}");
        return patient.ToDto();
    }

    public async Task<PagedResult<PatientDto>> GetFilteredPatientsAsync(PatientFilterDto filter)
    {
        var (pageNumber, pageSize, offset) = FhirPaginationHelper.Normalize(
            filter.PageNumber, filter.PageSize);

        var searchParams = new SearchParams();
        
        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(filter.Name))
            searchParams.Add("name", filter.Name.Trim());
        
        if (filter.Gender.HasValue)
            searchParams.Add("gender", filter.Gender.Value.ToString().ToLowerInvariant());
        
        // Paginación
        searchParams.Count = pageSize;
        searchParams.Add("_offset", offset.ToString());
        searchParams.Add("_total", "accurate");

        var bundle = await _fhirClient.SearchAsync<FhirPatient>(searchParams);
        
        var pagedResult = FhirPaginationHelper.ToPagedResult<FhirPatient>(
            bundle, pageNumber, pageSize);

        return new PagedResult<PatientDto>
        {
            Items = pagedResult.Items.Select(p => p.ToDto()).ToList(),
            Pagination = pagedResult.Pagination
        };
    }
}
```

### 5.5 DTOs (Data Transfer Objects)

Los DTOs definen la estructura de datos para la API:

```csharp
// DTO para crear paciente
public class CreatePatientDto
{
    [Required]
    public List<HumanNameDto> Name { get; set; }
    
    public string Gender { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    public List<IdentifierDto> Identifier { get; set; }
    
    public List<ContactPointDto> Telecom { get; set; }
    
    public List<AddressDto> Address { get; set; }
}

// DTO para actualizar paciente
public class UpdatePatientDto
{
    public List<HumanNameDto> Name { get; set; }
    public string Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public List<ContactPointDto> Telecom { get; set; }
    public List<AddressDto> Address { get; set; }
}

// DTO de respuesta
public class PatientDto
{
    public string Id { get; set; }
    public List<HumanNameDto> Name { get; set; }
    public string Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public List<IdentifierDto> Identifier { get; set; }
    public List<ContactPointDto> Telecom { get; set; }
    public List<AddressDto> Address { get; set; }
    public bool? Active { get; set; }
}

// DTO para filtros
public class PatientFilterDto : PaginationDto
{
    public string Name { get; set; }
    public GenderEnum? Gender { get; set; }
    public string IdentifierType { get; set; }
    public string IdentifierValue { get; set; }
    public DateTime? BirthDate { get; set; }
    public bool? Active { get; set; }
}
```

### 5.6 Extensions (Mapeo)

Las extensiones convierten entre DTOs y entidades FHIR:

```csharp
public static class PatientExtensions
{
    // DTO -> FHIR Patient
    public static FhirPatient ToFhirPatient(this CreatePatientDto dto)
    {
        var patient = new FhirPatient
        {
            Active = true,
            Name = dto.Name?.Select(n => new HumanName
            {
                Use = HumanName.NameUse.Official,
                Family = n.Family,
                Given = n.Given
            }).ToList(),
            Gender = ParseGender(dto.Gender),
            BirthDate = dto.BirthDate?.ToString("yyyy-MM-dd"),
            Identifier = dto.Identifier?.Select(i => new Identifier
            {
                System = i.System,
                Value = i.Value,
                Type = new CodeableConcept
                {
                    Text = i.Type
                }
            }).ToList(),
            Telecom = dto.Telecom?.Select(t => new ContactPoint
            {
                System = ParseContactPointSystem(t.System),
                Value = t.Value,
                Use = ParseContactPointUse(t.Use)
            }).ToList(),
            Address = dto.Address?.Select(a => new Address
            {
                Use = ParseAddressUse(a.Use),
                Type = ParseAddressType(a.Type),
                Line = a.Line,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList()
        };

        return patient;
    }

    // FHIR Patient -> DTO
    public static PatientDto ToDto(this FhirPatient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            Name = patient.Name?.Select(n => new HumanNameDto
            {
                Family = n.Family,
                Given = n.Given?.ToArray()
            }).ToList(),
            Gender = patient.Gender?.ToString(),
            BirthDate = DateTime.TryParse(patient.BirthDate, out var date) ? date : null,
            Identifier = patient.Identifier?.Select(i => new IdentifierDto
            {
                System = i.System,
                Value = i.Value,
                Type = i.Type?.Text
            }).ToList(),
            Telecom = patient.Telecom?.Select(t => new ContactPointDto
            {
                System = t.System?.ToString(),
                Value = t.Value,
                Use = t.Use?.ToString()
            }).ToList(),
            Address = patient.Address?.Select(a => new AddressDto
            {
                Use = a.Use?.ToString(),
                Type = a.Type?.ToString(),
                Line = a.Line?.ToList(),
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList(),
            Active = patient.Active
        };
    }

    // Aplicar actualización
    public static FhirPatient ApplyUpdate(this FhirPatient patient, UpdatePatientDto dto)
    {
        if (dto.Name != null)
            patient.Name = dto.Name.Select(n => new HumanName
            {
                Family = n.Family,
                Given = n.Given
            }).ToList();

        if (!string.IsNullOrEmpty(dto.Gender))
            patient.Gender = ParseGender(dto.Gender);

        if (dto.BirthDate.HasValue)
            patient.BirthDate = dto.BirthDate.Value.ToString("yyyy-MM-dd");

        if (dto.Telecom != null)
            patient.Telecom = dto.Telecom.Select(t => new ContactPoint
            {
                System = ParseContactPointSystem(t.System),
                Value = t.Value,
                Use = ParseContactPointUse(t.Use)
            }).ToList();

        if (dto.Address != null)
            patient.Address = dto.Address.Select(a => new Address
            {
                Use = ParseAddressUse(a.Use),
                Type = ParseAddressType(a.Type),
                Line = a.Line,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country
            }).ToList();

        return patient;
    }
}
```

### 5.7 Paginación

Helper para paginación consistente:

```csharp
public static class FhirPaginationHelper
{
    public static (int pageNumber, int pageSize, int offset) Normalize(
        int? pageNumber, int? pageSize)
    {
        var page = pageNumber ?? 1;
        var size = pageSize ?? 10;
        
        if (page < 1) page = 1;
        if (size < 1) size = 10;
        if (size > 100) size = 100;
        
        var offset = (page - 1) * size;
        
        return (page, size, offset);
    }

    public static PagedResult<T> ToPagedResult<T>(
        Bundle bundle, int pageNumber, int pageSize) where T : Resource
    {
        var items = bundle.Entry?
            .Select(e => e.Resource as T)
            .Where(r => r != null)
            .ToList() ?? new List<T>();

        var total = bundle.Total ?? 0;
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return new PagedResult<T>
        {
            Items = items,
            Pagination = new PaginationInfo
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            }
        };
    }
}
```

---

## 6. Frontend - Web

### 6.1 Arquitectura del Frontend

El frontend sigue una arquitectura modular basada en features:

```
Feature-Based Architecture
├── features/          # Módulos funcionales
│   ├── patients/     # Todo relacionado a pacientes
│   │   ├── components/   # Componentes específicos
│   │   ├── hooks/        # Hooks personalizados
│   │   ├── pages/        # Páginas del módulo
│   │   ├── routers/      # Rutas del módulo
│   │   └── store/        # Estado del módulo
│   └── ...
├── shared/           # Recursos compartidos
│   ├── components/   # Componentes reutilizables
│   ├── hooks/        # Hooks globales
│   └── utils/        # Utilidades
└── api/              # Cliente API generado
```

### 6.2 Configuración de Vite

**vite.config.ts**:

```typescript
import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [
    tailwindcss(),
    react()
  ],
  build: {
    rollupOptions: {
      onwarn(warning, warn) {
        if (warning.code === 'UNUSED_EXTERNAL_IMPORT') return
        warn(warning)
      }
    }
  },
  server: {
    host: true,
    port: parseInt(process.env.PORT ?? "5173"),
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL || 'http://localhost:5226',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
```

### 6.3 Generación de Cliente API con Orval

**orval.config.cjs**:

```javascript
require('dotenv').config();

module.exports = {
  api: {
    input: {
      target: process.env.ORVAL_API_URL, // URL del Swagger
    },
    output: {
      mode: "tags-split",              // Un archivo por controller
      target: "src/api",               // Carpeta de salida
      client: "react-query",           // Usar TanStack Query
      schemas: "src/api/models",       // Modelos TypeScript
      prettier: true,
      override: {
        mutator: {
          path: "./src/api/mutator/customInstance.ts",
          name: "customInstance",
        },
        shouldSplitQueryKey: true,     // Query keys como arrays
      },
    },
  },
};
```

**Mutator personalizado** (customInstance.ts):

```typescript
import Axios, { AxiosRequestConfig } from 'axios';

export const AXIOS_INSTANCE = Axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
});

// Interceptor para agregar token
AXIOS_INSTANCE.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor para manejar errores
AXIOS_INSTANCE.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Redirigir a login
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const customInstance = <T>(
  config: AxiosRequestConfig
): Promise<T> => {
  const source = Axios.CancelToken.source();
  const promise = AXIOS_INSTANCE({
    ...config,
    cancelToken: source.token,
  }).then(({ data }) => data);

  // @ts-ignore
  promise.cancel = () => {
    source.cancel('Query was cancelled');
  };

  return promise;
};
```

**Uso de hooks generados**:

```typescript
import { useGetPatients, useCreatePatient } from '@/api/patients';

function PatientsList() {
  // Query para obtener pacientes
  const { data, isLoading, error } = useGetPatients({
    pageNumber: 1,
    pageSize: 10,
    name: 'Juan'
  });

  // Mutation para crear paciente
  const createMutation = useCreatePatient();

  const handleCreate = async (patientData) => {
    try {
      await createMutation.mutateAsync(patientData);
      // Éxito
    } catch (error) {
      // Error
    }
  };

  if (isLoading) return <Spinner />;
  if (error) return <Error message={error.message} />;

  return (
    <div>
      {data?.items.map(patient => (
        <PatientCard key={patient.id} patient={patient} />
      ))}
    </div>
  );
}
```

### 6.4 Autenticación con Keycloak

**Configuración de Keycloak** (auth/keycloak.ts):

```typescript
import Keycloak from 'keycloak-js';

const keycloakConfig = {
  url: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
};

const keycloak = new Keycloak(keycloakConfig);

export default keycloak;
```

**Provider de autenticación**:

```typescript
import { ReactKeycloakProvider } from '@react-keycloak/web';
import keycloak from './auth/keycloak';

function App() {
  return (
    <ReactKeycloakProvider
      authClient={keycloak}
      initOptions={{
        onLoad: 'login-required',
        checkLoginIframe: false,
      }}
      onTokens={(tokens) => {
        // Guardar token
        localStorage.setItem('token', tokens.token);
      }}
    >
      <AppRouter />
    </ReactKeycloakProvider>
  );
}
```

**Hook para obtener usuario**:

```typescript
import { useKeycloak } from '@react-keycloak/web';

export function useAuth() {
  const { keycloak, initialized } = useKeycloak();

  return {
    user: keycloak.tokenParsed,
    isAuthenticated: keycloak.authenticated,
    roles: keycloak.tokenParsed?.realm_access?.roles || [],
    logout: () => keycloak.logout(),
    login: () => keycloak.login(),
  };
}
```

### 6.5 Autorización con CASL

**Definición de habilidades** (auth/abilities.ts):

```typescript
import { AbilityBuilder, Ability } from '@casl/ability';

export type Actions = 'create' | 'read' | 'update' | 'delete' | 'manage';
export type Subjects = 'Patient' | 'Healthcare' | 'Invoice' | 'Report' | 'all';

export type AppAbility = Ability<[Actions, Subjects]>;

export function defineAbilitiesFor(roles: string[]): AppAbility {
  const { can, cannot, build } = new AbilityBuilder<AppAbility>(Ability);

  if (roles.includes('admin')) {
    can('manage', 'all'); // Admin puede todo
  }

  if (roles.includes('cashier')) {
    can('read', 'Patient');
    can('create', 'Patient');
    can('update', 'Patient');
    can('create', 'Invoice');
    can('read', 'Invoice');
    can('read', 'Healthcare');
  }

  if (roles.includes('auditor')) {
    can('read', 'all');
    cannot('create', 'all');
    cannot('update', 'all');
    cannot('delete', 'all');
  }

  if (roles.includes('ti')) {
    can('manage', 'User');
    can('manage', 'Location');
    can('manage', 'Organization');
  }

  return build();
}
```

**Uso de CASL**:

```typescript
import { Can } from '@casl/react';
import { useAbility } from './auth/abilities';

function PatientActions({ patient }) {
  const ability = useAbility();

  return (
    <div>
      <Can I="update" a="Patient" ability={ability}>
        <Button onClick={() => editPatient(patient)}>Editar</Button>
      </Can>
      
      <Can I="delete" a="Patient" ability={ability}>
        <Button onClick={() => deletePatient(patient)}>Eliminar</Button>
      </Can>
    </div>
  );
}
```

### 6.6 Gestión de Estado con Zustand

**Store de ejemplo** (store/patientStore.ts):

```typescript
import { create } from 'zustand';
import { PatientDto } from '@/api/models';

interface PatientState {
  selectedPatient: PatientDto | null;
  searchTerm: string;
  setSelectedPatient: (patient: PatientDto | null) => void;
  setSearchTerm: (term: string) => void;
  clearSelection: () => void;
}

export const usePatientStore = create<PatientState>((set) => ({
  selectedPatient: null,
  searchTerm: '',
  setSelectedPatient: (patient) => set({ selectedPatient: patient }),
  setSearchTerm: (term) => set({ searchTerm: term }),
  clearSelection: () => set({ selectedPatient: null, searchTerm: '' }),
}));
```

**Uso del store**:

```typescript
import { usePatientStore } from '@/store/patientStore';

function PatientSelector() {
  const { selectedPatient, setSelectedPatient } = usePatientStore();

  return (
    <Select
      value={selectedPatient?.id}
      onChange={(id) => {
        const patient = patients.find(p => p.id === id);
        setSelectedPatient(patient);
      }}
    >
      {patients.map(p => (
        <Option key={p.id} value={p.id}>{p.name}</Option>
      ))}
    </Select>
  );
}
```

### 6.7 Formularios con Formik y Yup

**Esquema de validación**:

```typescript
import * as Yup from 'yup';

export const patientSchema = Yup.object().shape({
  name: Yup.array()
    .of(
      Yup.object().shape({
        family: Yup.string().required('Apellido es requerido'),
        given: Yup.array()
          .of(Yup.string())
          .min(1, 'Al menos un nombre es requerido'),
      })
    )
    .min(1, 'Nombre es requerido'),
  gender: Yup.string()
    .oneOf(['male', 'female', 'other', 'unknown'])
    .required('Género es requerido'),
  birthDate: Yup.date()
    .max(new Date(), 'Fecha de nacimiento no puede ser futura')
    .required('Fecha de nacimiento es requerida'),
  identifier: Yup.array()
    .of(
      Yup.object().shape({
        type: Yup.string().required('Tipo de identificador requerido'),
        value: Yup.string().required('Valor de identificador requerido'),
      })
    )
    .min(1, 'Al menos un identificador es requerido'),
});
```

**Formulario con Formik**:

```typescript
import { Formik, Form, Field } from 'formik';
import { patientSchema } from './validations';

function PatientForm({ onSubmit, initialValues }) {
  return (
    <Formik
      initialValues={initialValues}
      validationSchema={patientSchema}
      onSubmit={onSubmit}
    >
      {({ errors, touched, isSubmitting }) => (
        <Form>
          <div>
            <label>Apellido</label>
            <Field name="name[0].family" />
            {errors.name?.[0]?.family && touched.name?.[0]?.family && (
              <div className="error">{errors.name[0].family}</div>
            )}
          </div>

          <div>
            <label>Nombre</label>
            <Field name="name[0].given[0]" />
            {errors.name?.[0]?.given && touched.name?.[0]?.given && (
              <div className="error">{errors.name[0].given}</div>
            )}
          </div>

          <div>
            <label>Género</label>
            <Field as="select" name="gender">
              <option value="">Seleccione...</option>
              <option value="male">Masculino</option>
              <option value="female">Femenino</option>
              <option value="other">Otro</option>
            </Field>
            {errors.gender && touched.gender && (
              <div className="error">{errors.gender}</div>
            )}
          </div>

          <div>
            <label>Fecha de Nacimiento</label>
            <Field type="date" name="birthDate" />
            {errors.birthDate && touched.birthDate && (
              <div className="error">{errors.birthDate}</div>
            )}
          </div>

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Guardando...' : 'Guardar'}
          </button>
        </Form>
      )}
    </Formik>
  );
}
```

### 6.8 Routing

**Configuración de rutas** (routers/AppRouter.tsx):

```typescript
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '@/auth';

// Layouts
import MainLayout from '@/shared/layouts/MainLayout';
import AuthLayout from '@/shared/layouts/AuthLayout';

// Pages
import Dashboard from '@/features/dashboard/pages/Dashboard';
import PatientsList from '@/features/patients/pages/PatientsList';
import CreatePatient from '@/features/patients/pages/CreatePatient';
// ... más imports

function AppRouter() {
  const { isAuthenticated, roles } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path="/" element={<Dashboard />} />
          
          {/* Rutas de Pacientes */}
          <Route path="/patients">
            <Route index element={<PatientsList />} />
            <Route path="create" element={<CreatePatient />} />
            <Route path=":id/edit" element={<EditPatient />} />
          </Route>

          {/* Rutas de Servicios */}
          <Route path="/healthcares">
            <Route index element={<HealthcaresList />} />
            <Route path="create" element={<CreateHealthcare />} />
          </Route>

          {/* Rutas de Ingresos - Solo para cashier y admin */}
          {(roles.includes('cashier') || roles.includes('admin')) && (
            <Route path="/incomes">
              <Route index element={<IncomesList />} />
              <Route path="create" element={<CreateIncome />} />
              <Route path="close" element={<CloseCashier />} />
            </Route>
          )}

          {/* Rutas de Reportes - Solo para admin y auditor */}
          {(roles.includes('admin') || roles.includes('auditor')) && (
            <Route path="/reports">
              <Route index element={<ReportsList />} />
              <Route path="create" element={<CreateReport />} />
            </Route>
          )}
        </Route>

        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRouter;
```

---

## 7. Base de Datos

### 7.1 Modelo de Datos

El sistema utiliza tres bases de datos:

1. **PostgreSQL (SIGREF)**: Datos transaccionales del sistema
2. **PostgreSQL (HAPI)**: Recursos FHIR
3. **MongoDB**: Logs y auditoría

### 7.2 Esquema de Base de Datos SIGREF

**Entidades Principales**:

```csharp
// HospitalPropertiesEntity - Propiedades del hospital
public class HospitalPropertiesEntity
{
    public Guid Id { get; set; }
    public string HospitalName { get; set; }
    public string LogoUrl { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string TaxId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// InvoiceEntity - Facturas
public class InvoiceEntity
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
    public Guid SerieId { get; set; }
    public InvoiceSerieEntity Serie { get; set; }
    public string PatientId { get; set; } // FHIR Patient ID
    public string PatientName { get; set; }
    public string PatientIdentifier { get; set; }
    public DateTime IssueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public List<InvoiceItemEntity> Items { get; set; }
    public Guid? CashierSessionId { get; set; }
    public CashierSessionEntity CashierSession { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

// InvoiceItemEntity - Items de factura
public class InvoiceItemEntity
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public InvoiceEntity Invoice { get; set; }
    public string HealthcareServiceId { get; set; } // FHIR HealthcareService ID
    public string ServiceName { get; set; }
    public string ServiceCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
}

// CashierSessionEntity - Sesiones de caja
public class CashierSessionEntity
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public ShiftEntity Shift { get; set; }
    public string CashierId { get; set; } // FHIR Practitioner ID
    public string CashierName { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public decimal? DeclaredAmount { get; set; }
    public decimal? Difference { get; set; }
    public string Status { get; set; } // Open, Closed, PendingCorrection
    public string Notes { get; set; }
    public List<InvoiceEntity> Invoices { get; set; }
}

// ShiftEntity - Turnos
public class ShiftEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; }
    public List<CashierSessionEntity> CashierSessions { get; set; }
}

// HealthService - Servicios médicos (catálogo local)
public class HealthService
{
    public Guid Id { get; set; }
    public string FhirId { get; set; } // ID en HAPI FHIR
    public string Name { get; set; }
    public string Code { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**DbContext**:

```csharp
public class SIGREFContext : DbContext
{
    public SIGREFContext(DbContextOptions<SIGREFContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<HospitalPropertiesEntity> HospitalProperties { get; set; }
    public DbSet<MediaFileEntity> MediaFiles { get; set; }
    public DbSet<HealthService> HealthServices { get; set; }
    public DbSet<InvoiceEntity> Invoices { get; set; }
    public DbSet<InvoiceSerieEntity> InvoiceSeries { get; set; }
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; }
    public DbSet<ShiftEntity> Shifts { get; set; }
    public DbSet<CashierSessionEntity> CashierSessions { get; set; }
    public DbSet<ReportHistoryEntity> ReportHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar configuraciones automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SIGREFContext).Assembly);
    }
}
```

### 7.3 Migraciones

**Crear migración**:

```bash
cd BE/SIGREF.API
dotnet ef migrations add NombreDeLaMigracion
```

**Aplicar migraciones**:

```bash
dotnet ef database update
```

**Revertir migración**:

```bash
dotnet ef database update NombreMigracionAnterior
```

**Eliminar última migración**:

```bash
dotnet ef migrations remove
```

### 7.4 Seeding

**Seeder principal** (SIGREFSeeder.cs):

```csharp
public class SIGREFSeeder
{
    private readonly SIGREFContext _context;
    private readonly ILogger<SIGREFSeeder> _logger;

    public SIGREFSeeder(SIGREFContext context, ILogger<SIGREFSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed Shifts
            if (!await _context.Shifts.AnyAsync())
            {
                var shifts = new List<ShiftEntity>
                {
                    new ShiftEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Turno Matutino",
                        StartTime = new TimeSpan(7, 0, 0),
                        EndTime = new TimeSpan(15, 0, 0),
                        IsActive = true
                    },
                    new ShiftEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Turno Vespertino",
                        StartTime = new TimeSpan(15, 0, 0),
                        EndTime = new TimeSpan(23, 0, 0),
                        IsActive = true
                    }
                };

                await _context.Shifts.AddRangeAsync(shifts);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Shifts seeded successfully");
            }

            // Seed Hospital Properties
            if (!await _context.HospitalProperties.AnyAsync())
            {
                var hospitalProps = new HospitalPropertiesEntity
                {
                    Id = Guid.NewGuid(),
                    HospitalName = "Hospital General",
                    Address = "Dirección del Hospital",
                    Phone = "1234-5678",
                    Email = "info@hospital.com",
                    TaxId = "0000-000000-000-0",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.HospitalProperties.AddAsync(hospitalProps);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Hospital properties seeded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
```

---

## 8. Autenticación y Autorización

### 8.1 Keycloak

**Configuración de Realm**:

1. Crear realm `sigref`
2. Configurar clientes:
   - `frontend` (public client)
   - `sigref-api` (confidential client)
   - `hapi-fhir` (confidential client)

**Roles del Sistema**:

```
Realm Roles:
├── admin       (Administrador completo)
├── cashier     (Auxiliar de caja)
├── auditor     (Auditor)
└── ti          (Técnico de informática)
```

### 8.2 JWT Token

**Estructura del Token**:

```json
{
  "exp": 1234567890,
  "iat": 1234567890,
  "jti": "uuid",
  "iss": "https://keycloak.example.com/auth/realms/sigref",
  "aud": "sigref-api",
  "sub": "user-uuid",
  "typ": "Bearer",
  "azp": "frontend",
  "session_state": "uuid",
  "acr": "1",
  "realm_access": {
    "roles": ["admin", "cashier"]
  },
  "resource_access": {
    "sigref-api": {
      "roles": ["admin"]
    }
  },
  "scope": "openid profile email",
  "email_verified": true,
  "name": "Juan Pérez",
  "preferred_username": "jperez",
  "given_name": "Juan",
  "family_name": "Pérez",
  "email": "jperez@example.com"
}
```

### 8.3 Autorización en Backend

**Atributos de autorización**:

```csharp
// Requiere autenticación
[Authorize(AuthenticationSchemes = "Bearer")]

// Requiere rol específico
[Authorize(Roles = RolesConstants.admin)]

// Requiere uno de varios roles
[Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]

// Permitir acceso anónimo
[AllowAnonymous]
```

**Ejemplo en controlador**:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class InvoicesController : ControllerBase
{
    // Todos los usuarios autenticados
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }

    // Solo admin y auditor
    [HttpGet("reports")]
    [Authorize(Roles = $"{RolesConstants.admin},{RolesConstants.auditor}")]
    public async Task<IActionResult> GetReports()
    {
        // ...
    }

    // Solo admin
    [HttpDelete("{id}")]
    [Authorize(Roles = RolesConstants.admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // ...
    }
}
```

### 8.4 Autorización en Frontend

**Protección de rutas**:

```typescript
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/auth';

function ProtectedRoute({ children, requiredRoles }) {
  const { isAuthenticated, roles } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  if (requiredRoles && !requiredRoles.some(role => roles.includes(role))) {
    return <Navigate to="/unauthorized" />;
  }

  return children;
}

// Uso
<Route
  path="/admin"
  element={
    <ProtectedRoute requiredRoles={['admin']}>
      <AdminPanel />
    </ProtectedRoute>
  }
/>
```

**Protección de componentes**:

```typescript
import { Can } from '@casl/react';

function PatientActions() {
  return (
    <div>
      <Can I="create" a="Patient">
        <Button>Crear Paciente</Button>
      </Can>
      
      <Can I="delete" a="Patient">
        <Button>Eliminar</Button>
      </Can>
    </div>
  );
}
```

---

## 9. Integración FHIR

### 9.1 HAPI FHIR Server

**Configuración**:

```yaml
# docker-compose.yml
hapifhir:
  image: hapiproject/hapi:latest
  environment:
    spring.datasource.url: jdbc:postgresql://postgres:5432/hapi
    spring.datasource.username: sigref_user
    spring.datasource.password: ${POSTGRES_PASSWORD}
    hapi.fhir.fhir_version: R4
    hapi.fhir.server_address: https://example.com/fhir
    hapi.fhir.security.oauth_enabled: "true"
    hapi.fhir.security.oauth_issuer_url: https://example.com/auth/realms/sigref
```

### 9.2 Recursos FHIR Utilizados

**Patient** - Pacientes:
```json
{
  "resourceType": "Patient",
  "id": "123",
  "identifier": [
    {
      "system": "http://hospital.com/identifiers/national-id",
      "value": "0801-1990-12345"
    }
  ],
  "name": [
    {
      "use": "official",
      "family": "Pérez",
      "given": ["Juan", "Carlos"]
    }
  ],
  "gender": "male",
  "birthDate": "1990-01-15",
  "telecom": [
    {
      "system": "phone",
      "value": "9999-9999",
      "use": "mobile"
    }
  ],
  "address": [
    {
      "use": "home",
      "line": ["Colonia Example, Calle 123"],
      "city": "Tegucigalpa",
      "state": "Francisco Morazán",
      "postalCode": "11101",
      "country": "HN"
    }
  ]
}
```

**Practitioner** - Empleados:
```json
{
  "resourceType": "Practitioner",
  "id": "456",
  "identifier": [
    {
      "system": "http://hospital.com/identifiers/employee-id",
      "value": "EMP-001"
    }
  ],
  "name": [
    {
      "family": "García",
      "given": ["María"]
    }
  ],
  "telecom": [
    {
      "system": "email",
      "value": "mgarcia@hospital.com"
    }
  ],
  "qualification": [
    {
      "code": {
        "coding": [
          {
            "system": "http://terminology.hl7.org/CodeSystem/v2-0360",
            "code": "MD",
            "display": "Doctor of Medicine"
          }
        ]
      }
    }
  ]
}
```

**HealthcareService** - Servicios médicos:
```json
{
  "resourceType": "HealthcareService",
  "id": "789",
  "active": true,
  "name": "Consulta General",
  "category": [
    {
      "coding": [
        {
          "system": "http://terminology.hl7.org/CodeSystem/service-category",
          "code": "17",
          "display": "General Practice"
        }
      ]
    }
  ],
  "type": [
    {
      "coding": [
        {
          "system": "http://terminology.hl7.org/CodeSystem/service-type",
          "code": "124",
          "display": "General Practice"
        }
      ]
    }
  ],
  "specialty": [
    {
      "coding": [
        {
          "system": "http://snomed.info/sct",
          "code": "394814009",
          "display": "General practice"
        }
      ]
    }
  ]
}
```

### 9.3 Cliente FHIR en .NET

**FhirService**:

```csharp
public class FhirService
{
    private readonly IConfiguration _configuration;
    private FhirClient _fhirClient;

    public FhirService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public FhirClient GetFhirClient()
    {
        if (_fhirClient == null)
        {
            var fhirUrl = _configuration["HAPIFHIR_HTTP"];
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = false,
                Timeout = 30000
            };

            _fhirClient = new FhirClient(fhirUrl, settings);
        }

        return _fhirClient;
    }
}
```

---

## 10. Observabilidad y Monitoreo

### 10.1 OpenTelemetry

**Arquitectura de Telemetría**:

```
Servicios (.NET, Java, Keycloak)
    │
    ▼ OTLP gRPC (puerto 4317)
OpenTelemetry Collector
    │
    ├──→ Aspire Dashboard (tiempo real)
    │
    └──→ Seq (persistencia y búsqueda)
```

**Configuración del Collector** (otel-collector-config.yaml):

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024

exporters:
  # Aspire Dashboard (tiempo real)
  otlp/aspire:
    endpoint: env-dashboard:18889
    tls:
      insecure: true

  # Seq (persistencia)
  otlphttp/seq:
    endpoint: http://seq:80/ingest/otlp
    headers:
      X-Seq-ApiKey: ${SEQ_API_KEY}

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire, otlphttp/seq]
    
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
    
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire, otlphttp/seq]
```

### 10.2 Aspire Dashboard

**Acceso**: `https://tu-dominio.com/dashboard`

**Características**:
- Visualización de traces en tiempo real
- Métricas de rendimiento
- Logs estructurados
- Dependencias entre servicios
- Health checks

**Configuración**:

```yaml
env-dashboard:
  image: mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest
  environment:
    # Autenticación con Keycloak
    Dashboard__Frontend__AuthMode: "OpenIdConnect"
    Dashboard__Frontend__OpenIdConnect__RequiredClaimType: "realm_roles"
    Dashboard__Frontend__OpenIdConnect__RequiredClaimValue: "ti"
    Authentication__Schemes__OpenIdConnect__Authority: "http://keycloak:8080/auth/realms/sigref"
    Authentication__Schemes__OpenIdConnect__ClientId: "aspire-dashboard"
    Authentication__Schemes__OpenIdConnect__ClientSecret: "${DASHBOARD_CLIENT_SECRET}"
    # OTLP sin autenticación (interno)
    Dashboard__Otlp__AuthMode: "Unsecured"
```

### 10.3 Seq (Log Aggregation)

**Acceso**: `http://servidor:5341`

**Características**:
- Búsqueda avanzada de logs
- Filtros y queries
- Alertas configurables
- Retención de logs
- Dashboards personalizados

**Queries útiles**:

```sql
-- Errores en las últimas 24 horas
@Level = 'Error' and @Timestamp > Now() - 1d

-- Logs de un usuario específico
UserId = 'user-uuid'

-- Operaciones lentas (> 1 segundo)
@Properties.Duration > 1000

-- Errores de autenticación
@Message like '%authentication%' and @Level = 'Error'

-- Logs de un servicio específico
ServiceName = 'sigref-api'
```

### 10.4 Logging en .NET

**Configuración**:

```csharp
// Program.cs
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});
```

**Uso en servicios**:

```csharp
public class PatientService
{
    private readonly ILogger<PatientService> _logger;

    public PatientService(ILogger<PatientService> logger)
    {
        _logger = logger;
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto)
    {
        _logger.LogInformation("Creating patient: {PatientName}", dto.Name);

        try
        {
            var patient = dto.ToFhirPatient();
            var created = await _fhirClient.CreateAsync(patient);
            
            _logger.LogInformation(
                "Patient created successfully: {PatientId}", 
                created.Id);
            
            return created.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error creating patient: {PatientName}", 
                dto.Name);
            throw;
        }
    }
}
```

### 10.5 Auditoría

**Sistema de Auditoría**:

El sistema registra automáticamente todas las operaciones:

```csharp
// Modelo de auditoría
public class AuditLog
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; }
    public string Resource { get; set; }
    public string ResourceId { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public int StatusCode { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public Dictionary<string, object> Changes { get; set; }
}
```

**Middleware de auditoría**:

```csharp
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditService _auditService;

    public AuditMiddleware(RequestDelegate next, IAuditService auditService)
    {
        _next = next;
        _auditService = auditService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        // Capturar request body si es necesario
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalMilliseconds;

        // Registrar auditoría
        await _auditService.LogAsync(new AuditLog
        {
            Timestamp = startTime,
            UserId = context.User?.FindFirst("sub")?.Value,
            UserName = context.User?.Identity?.Name,
            Action = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = context.Response.StatusCode,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"],
            Duration = duration
        });

        // Restaurar response body
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
```

---

## 11. Despliegue

### 11.1 Requisitos de Infraestructura

**Servidor**:
- CPU: 4 cores mínimo
- RAM: 8 GB mínimo (16 GB recomendado)
- Disco: 50 GB mínimo (SSD recomendado)
- OS: Linux (Ubuntu 22.04 LTS recomendado)

**Software**:
- Docker 24.0+
- Docker Compose 2.20+
- Cloudflare Tunnel (cloudflared)

### 11.2 Variables de Entorno

**Archivo .env.production**:

```bash
# PostgreSQL
POSTGRES_USER=sigref_user
POSTGRES_PASSWORD=<contraseña-segura>
POSTGRES_DB=postgres

# MongoDB
MONGODB_USER=mongo_user
MONGODB_PASSWORD=<contraseña-segura>

# Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_PASSWORD=<contraseña-segura>
KC_HOSTNAME_URL=https://tu-dominio.com/auth

# HAPI FHIR
HAPI_SERVER_ADDRESS=https://tu-dominio.com/fhir
HAPI_OAUTH_ISSUER=https://tu-dominio.com/auth/realms/sigref
HAPI_OAUTH_CLIENT_SECRET=<secreto-del-cliente>

# Frontend
FRONTEND_KEYCLOAK_URL=https://tu-dominio.com/auth
FRONTEND_API_URL=https://tu-dominio.com/api

# Seq
SEQ_ADMIN_PASSWORD=<contraseña-segura>
SEQ_API_KEY=<api-key-generada>

# Aspire Dashboard
DASHBOARD_CLIENT_SECRET=<secreto-del-cliente>
DASHBOARD_REQUIRED_ROLE=ti

# OpenTelemetry
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

### 11.3 Despliegue con Docker Compose

**1. Clonar repositorio**:

```bash
git clone https://github.com/tu-org/SIGREF.git
cd SIGREF
```

**2. Configurar variables de entorno**:

```bash
cp .env.production .env
nano .env  # Editar con valores reales
```

**3. Crear archivo de contraseñas para nginx**:

```bash
sudo apt-get install apache2-utils
mkdir -p nginx
htpasswd -c nginx/.htpasswd admin
```

**4. Iniciar servicios**:

```bash
docker-compose up -d
```

**5. Verificar estado**:

```bash
docker-compose ps
docker-compose logs -f
```

### 11.4 Configuración de Cloudflare Tunnel

**1. Instalar cloudflared**:

```bash
wget https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
sudo dpkg -i cloudflared-linux-amd64.deb
```

**2. Autenticar**:

```bash
cloudflared tunnel login
```

**3. Crear tunnel**:

```bash
cloudflared tunnel create sigref
```

**4. Configurar tunnel** (~/.cloudflared/config.yml):

```yaml
tunnel: <TUNNEL-ID>
credentials-file: /home/<USER>/.cloudflared/<TUNNEL-ID>.json

ingress:
  - hostname: tu-dominio.com
    service: http://localhost:80
  - service: http_status:404
```

**5. Crear ruta DNS**:

```bash
cloudflared tunnel route dns sigref tu-dominio.com
```

**6. Instalar como servicio**:

```bash
sudo cloudflared service install
sudo systemctl start cloudflared
sudo systemctl enable cloudflared
```

### 11.5 Configuración Post-Despliegue

**1. Configurar Keycloak**:

- Acceder a `https://tu-dominio.com/auth/admin`
- Crear realm `sigref`
- Crear clientes (frontend, sigref-api, hapi-fhir, aspire-dashboard)
- Configurar roles
- Crear usuarios de prueba

**2. Configurar Seq**:

- Acceder a `http://servidor:5341`
- Login con admin y contraseña de SEQ_ADMIN_PASSWORD
- Crear API Key para el collector
- Agregar API Key a .env como SEQ_API_KEY
- Reiniciar otel-collector

**3. Verificar servicios**:

```bash
# Health checks
curl https://tu-dominio.com/health

# API
curl https://tu-dominio.com/api/health

# Frontend
curl https://tu-dominio.com/

# Keycloak
curl https://tu-dominio.com/auth/realms/sigref
```

### 11.6 Backup y Restauración

**Backup de PostgreSQL**:

```bash
# Backup completo
docker-compose exec postgres pg_dumpall -U sigref_user > backup_$(date +%Y%m%d_%H%M%S).sql

# Backup de base específica
docker-compose exec postgres pg_dump -U sigref_user sigref > sigref_backup.sql
```

**Restaurar PostgreSQL**:

```bash
cat backup.sql | docker-compose exec -T postgres psql -U sigref_user
```

**Backup de MongoDB**:

```bash
docker-compose exec mongodb mongodump \
  --username mongo_user \
  --password "$MONGODB_PASSWORD" \
  --authenticationDatabase admin \
  --out /backup

docker cp $(docker-compose ps -q mongodb):/backup ./mongodb_backup
```

**Restaurar MongoDB**:

```bash
docker cp ./mongodb_backup $(docker-compose ps -q mongodb):/restore
docker-compose exec mongodb mongorestore \
  --username mongo_user \
  --password "$MONGODB_PASSWORD" \
  --authenticationDatabase admin \
  /restore
```

---

## 12. Guía de Desarrollo

### 12.1 Configuración del Entorno de Desarrollo

**Requisitos**:
- .NET 9.0 SDK
- Node.js 20+ y npm
- Docker Desktop
- Visual Studio Code o Visual Studio 2022
- Git

**Clonar repositorio**:

```bash
git clone https://github.com/tu-org/SIGREF.git
cd SIGREF
```

**Configurar Backend**:

```bash
cd BE/SIGREF.API

# Restaurar paquetes
dotnet restore

# Crear base de datos local
dotnet ef database update

# Ejecutar
dotnet run
```

**Configurar Frontend**:

```bash
cd FE

# Instalar dependencias
npm install

# Generar cliente API
npm run orval

# Ejecutar en desarrollo
npm run dev
```

### 12.2 Estructura de Branches

```
main                    # Producción
├── develop            # Desarrollo
│   ├── feature/*      # Nuevas funcionalidades
│   ├── bugfix/*       # Corrección de bugs
│   └── hotfix/*       # Correcciones urgentes
```

**Convención de nombres**:

```bash
feature/SIGREF-123-add-patient-search
bugfix/SIGREF-456-fix-invoice-calculation
hotfix/SIGREF-789-critical-security-fix
```

### 12.3 Workflow de Desarrollo

**1. Crear feature branch**:

```bash
git checkout develop
git pull origin develop
git checkout -b feature/SIGREF-123-descripcion
```

**2. Desarrollar**:

```bash
# Hacer cambios
git add .
git commit -m "feat: descripción del cambio"
```

**3. Push y Pull Request**:

```bash
git push origin feature/SIGREF-123-descripcion
# Crear PR en GitHub hacia develop
```

**4. Code Review**:
- Al menos 1 aprobación requerida
- Pasar todos los checks de CI/CD
- Sin conflictos con develop

**5. Merge**:
- Squash and merge
- Eliminar branch después del merge

### 12.4 Convenciones de Código

**Backend (C#)**:

```csharp
// Nombres de clases: PascalCase
public class PatientService { }

// Nombres de métodos: PascalCase
public async Task<PatientDto> GetPatientByIdAsync(string id) { }

// Nombres de variables: camelCase
private readonly IPatientService _patientService;
var patientDto = new PatientDto();

// Constantes: PascalCase
public const string ResourceType = "Patient";

// Interfaces: I + PascalCase
public interface IPatientService { }

// Async methods: sufijo Async
public async Task<T> GetDataAsync() { }
```

**Frontend (TypeScript)**:

```typescript
// Nombres de componentes: PascalCase
function PatientList() { }

// Nombres de funciones: camelCase
function handleSubmit() { }

// Nombres de variables: camelCase
const patientData = {};

// Constantes: UPPER_SNAKE_CASE
const API_BASE_URL = 'https://api.example.com';

// Interfaces/Types: PascalCase
interface PatientDto { }
type PatientStatus = 'active' | 'inactive';

// Hooks personalizados: use + PascalCase
function usePatientData() { }
```

### 12.5 Agregar Nueva Funcionalidad

**Backend - Agregar nuevo endpoint**:

**1. Crear DTO**:

```csharp
// Dtos/NewFeature/CreateFeatureDto.cs
public class CreateFeatureDto
{
    [Required]
    public string Name { get; set; }
    
    public string Description { get; set; }
}

// Dtos/NewFeature/FeatureDto.cs
public class FeatureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**2. Crear Entidad**:

```csharp
// Database/Entity/FeatureEntity.cs
public class FeatureEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**3. Agregar DbSet**:

```csharp
// Database/SIGREFContext.cs
public DbSet<FeatureEntity> Features { get; set; }
```

**4. Crear Migración**:

```bash
dotnet ef migrations add AddFeatureEntity
dotnet ef database update
```

**5. Crear Servicio**:

```csharp
// Services/Feature/IFeatureService.cs
public interface IFeatureService
{
    Task<FeatureDto> CreateAsync(CreateFeatureDto dto);
    Task<FeatureDto> GetByIdAsync(Guid id);
    Task<PagedResult<FeatureDto>> GetAllAsync(PaginationDto pagination);
}

// Services/Feature/FeatureService.cs
public class FeatureService : IFeatureService
{
    private readonly SIGREFContext _context;
    
    public FeatureService(SIGREFContext context)
    {
        _context = context;
    }
    
    public async Task<FeatureDto> CreateAsync(CreateFeatureDto dto)
    {
        var entity = new FeatureEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Features.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity.ToDto();
    }
    
    // Implementar otros métodos...
}
```

**6. Registrar Servicio**:

```csharp
// Startup.cs
services.AddScoped<IFeatureService, FeatureService>();
```

**7. Crear Controlador**:

```csharp
// Controllers/Feature/FeatureController.cs
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class FeatureController : ControllerBase
{
    private readonly IFeatureService _featureService;
    
    public FeatureController(IFeatureService featureService)
    {
        _featureService = featureService;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFeatureDto dto)
    {
        var result = await _featureService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _featureService.GetByIdAsync(id);
        return Ok(result);
    }
}
```

**Frontend - Agregar nuevo módulo**:

**1. Generar cliente API**:

```bash
npm run orval
```

**2. Crear estructura del módulo**:

```
features/new-feature/
├── components/
│   ├── FeatureCard.tsx
│   └── FeatureForm.tsx
├── hooks/
│   └── useFeatureData.ts
├── pages/
│   ├── FeatureList.tsx
│   └── CreateFeature.tsx
├── routers/
│   └── FeatureRouter.tsx
└── store/
    └── featureStore.ts
```

**3. Crear componentes**:

```typescript
// pages/FeatureList.tsx
import { useGetFeatures } from '@/api/features';

export function FeatureList() {
  const { data, isLoading } = useGetFeatures({
    pageNumber: 1,
    pageSize: 10
  });

  if (isLoading) return <Spinner />;

  return (
    <div>
      <h1>Features</h1>
      {data?.items.map(feature => (
        <FeatureCard key={feature.id} feature={feature} />
      ))}
    </div>
  );
}

// pages/CreateFeature.tsx
import { useCreateFeature } from '@/api/features';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';

const schema = Yup.object().shape({
  name: Yup.string().required('Nombre requerido'),
  description: Yup.string()
});

export function CreateFeature() {
  const createMutation = useCreateFeature();

  const handleSubmit = async (values) => {
    try {
      await createMutation.mutateAsync(values);
      // Éxito
    } catch (error) {
      // Error
    }
  };

  return (
    <Formik
      initialValues={{ name: '', description: '' }}
      validationSchema={schema}
      onSubmit={handleSubmit}
    >
      <Form>
        <Field name="name" placeholder="Nombre" />
        <Field name="description" placeholder="Descripción" />
        <button type="submit">Crear</button>
      </Form>
    </Formik>
  );
}
```

**4. Agregar rutas**:

```typescript
// routers/FeatureRouter.tsx
import { Routes, Route } from 'react-router-dom';
import { FeatureList } from '../pages/FeatureList';
import { CreateFeature } from '../pages/CreateFeature';

export function FeatureRouter() {
  return (
    <Routes>
      <Route index element={<FeatureList />} />
      <Route path="create" element={<CreateFeature />} />
    </Routes>
  );
}

// Agregar a AppRouter.tsx
<Route path="/features/*" element={<FeatureRouter />} />
```

### 12.6 Testing

**Backend - Unit Tests**:

```csharp
using Xunit;
using Moq;

public class PatientServiceTests
{
    [Fact]
    public async Task CreatePatient_ValidData_ReturnsPatientDto()
    {
        // Arrange
        var mockFhirClient = new Mock<FhirClient>();
        var service = new PatientService(mockFhirClient.Object);
        var dto = new CreatePatientDto
        {
            Name = new List<HumanNameDto>
            {
                new HumanNameDto { Family = "Pérez", Given = new[] { "Juan" } }
            }
        };

        // Act
        var result = await service.CreatePatientAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pérez", result.Name[0].Family);
    }
}
```

**Frontend - Component Tests**:

```typescript
import { render, screen } from '@testing-library/react';
import { PatientCard } from './PatientCard';

describe('PatientCard', () => {
  it('renders patient name', () => {
    const patient = {
      id: '123',
      name: [{ family: 'Pérez', given: ['Juan'] }]
    };

    render(<PatientCard patient={patient} />);
    
    expect(screen.getByText('Juan Pérez')).toBeInTheDocument();
  });
});
```

### 12.7 Debugging

**Backend**:

```csharp
// Agregar breakpoints en Visual Studio/VS Code
// Ejecutar en modo debug: F5

// Logging para debugging
_logger.LogDebug("Variable value: {Value}", someVariable);
```

**Frontend**:

```typescript
// Console logging
console.log('Data:', data);
console.table(patients);

// React DevTools
// Instalar extensión de navegador

// Redux DevTools (si se usa Redux)
// Instalar extensión de navegador
```

---

## 13. API Reference

### 13.1 Endpoints Principales

**Base URL**: `https://tu-dominio.com/api`

**Autenticación**: Bearer Token (JWT)

### 13.2 Patients

**GET /api/patients**
- Descripción: Obtener lista de pacientes
- Autenticación: Requerida
- Query Parameters:
  - `pageNumber` (int, opcional): Número de página (default: 1)
  - `pageSize` (int, opcional): Tamaño de página (default: 10)
  - `name` (string, opcional): Filtrar por nombre
  - `gender` (string, opcional): Filtrar por género
  - `identifierValue` (string, opcional): Filtrar por identificador
- Response: 200 OK

```json
{
  "items": [
    {
      "id": "123",
      "name": [
        {
          "family": "Pérez",
          "given": ["Juan", "Carlos"]
        }
      ],
      "gender": "male",
      "birthDate": "1990-01-15",
      "active": true
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**POST /api/patients**
- Descripción: Crear nuevo paciente
- Autenticación: Requerida
- Request Body:

```json
{
  "name": [
    {
      "family": "Pérez",
      "given": ["Juan", "Carlos"]
    }
  ],
  "gender": "male",
  "birthDate": "1990-01-15",
  "identifier": [
    {
      "system": "http://hospital.com/identifiers/national-id",
      "value": "0801-1990-12345",
      "type": "DNI"
    }
  ],
  "telecom": [
    {
      "system": "phone",
      "value": "9999-9999",
      "use": "mobile"
    }
  ],
  "address": [
    {
      "use": "home",
      "line": ["Colonia Example, Calle 123"],
      "city": "Tegucigalpa",
      "state": "Francisco Morazán",
      "postalCode": "11101",
      "country": "HN"
    }
  ]
}
```

- Response: 201 Created

### 13.3 Healthcares (Servicios)

**GET /api/healthcares**
- Descripción: Obtener lista de servicios médicos
- Autenticación: Requerida
- Query Parameters:
  - `pageNumber` (int)
  - `pageSize` (int)
  - `name` (string)
  - `category` (string)
  - `active` (bool)
- Response: 200 OK

**POST /api/healthcares**
- Descripción: Crear nuevo servicio
- Autenticación: Requerida (admin, ti)
- Request Body:

```json
{
  "name": "Consulta General",
  "category": "consultation",
  "type": "general-practice",
  "specialty": "general-practice",
  "active": true
}
```

### 13.4 Invoices (Facturas)

**POST /api/invoices**
- Descripción: Crear nueva factura
- Autenticación: Requerida (cashier, admin)
- Request Body:

```json
{
  "patientId": "patient-fhir-id",
  "items": [
    {
      "healthcareServiceId": "service-fhir-id",
      "quantity": 1,
      "unitPrice": 100.00,
      "discount": 0
    }
  ],
  "paymentMethod": "cash",
  "discount": 0
}
```

- Response: 201 Created

**GET /api/invoices**
- Descripción: Obtener lista de facturas
- Autenticación: Requerida
- Query Parameters:
  - `pageNumber` (int)
  - `pageSize` (int)
  - `patientId` (string)
  - `startDate` (datetime)
  - `endDate` (datetime)
  - `status` (string)
- Response: 200 OK

### 13.5 CashierSessions (Sesiones de Caja)

**POST /api/cashiersessions/open**
- Descripción: Abrir sesión de caja
- Autenticación: Requerida (cashier, admin)
- Request Body:

```json
{
  "shiftId": "shift-guid",
  "initialAmount": 100.00
}
```

**POST /api/cashiersessions/{id}/close**
- Descripción: Cerrar sesión de caja
- Autenticación: Requerida (cashier, admin)
- Request Body:

```json
{
  "declaredAmount": 1500.00,
  "notes": "Cierre normal"
}
```

### 13.6 Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado |
| 204 | No Content - Eliminación exitosa |
| 400 | Bad Request - Datos inválidos |
| 401 | Unauthorized - No autenticado |
| 403 | Forbidden - Sin permisos |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error del servidor |

---

## 14. Troubleshooting

### 14.1 Problemas Comunes

**Error: Connection refused al iniciar servicios**

```bash
# Verificar que Docker esté corriendo
docker ps

# Verificar logs
docker-compose logs -f

# Reiniciar servicios
docker-compose restart
```

**Error: Migraciones pendientes**

```bash
cd BE/SIGREF.API
dotnet ef database update
```

**Error: Frontend no se conecta a la API**

```bash
# Verificar variables de entorno
cat FE/.env

# Verificar que VITE_API_URL esté correcta
# Regenerar cliente API
cd FE
npm run orval
```

**Error: Keycloak no autentica**

- Verificar que el realm esté creado
- Verificar que los clientes estén configurados
- Verificar URLs de redirect
- Verificar secretos de cliente

**Error: HAPI FHIR no responde**

```bash
# Verificar logs
docker-compose logs hapifhir

# Verificar conexión a PostgreSQL
docker-compose exec hapifhir curl localhost:8080/fhir/metadata
```

### 14.2 Logs y Diagnóstico

**Ver logs de un servicio**:

```bash
docker-compose logs -f sigref-api
docker-compose logs -f frontend
docker-compose logs -f keycloak
```

**Ver logs en Seq**:

- Acceder a `http://servidor:5341`
- Buscar por servicio, nivel, timestamp

**Ver métricas en Aspire Dashboard**:

- Acceder a `https://tu-dominio.com/dashboard`
- Revisar traces, métricas, logs

### 14.3 Contacto de Soporte

Para problemas técnicos, contactar al equipo de desarrollo:

- **Email**: dev@sigref.com
- **GitHub Issues**: https://github.com/tu-org/SIGREF/issues
- **Documentación**: https://docs.sigref.com

---

## Equipo de Desarrollo

- **Carlos Ovidio Dubón Pineda** - Backend Developer
- **Michael Andrey Galdamez Martinez** - Frontend Developer
- **Ever Josue Garcia Leonor** - Backend Developer
- **Danilo Isaac Vides Chicas** - Frontend Developer
- **Anthony Edward Miranda Fuentes** - Backend Developer
- **Hector Rene Martinez Vega** - Backend Developer
- **Erick Marley Arita** - Full Stack Developer
- **Josue David Diaz Rodriguez** - Full Stack Developer

---

## Licencia

Ver archivo [LICENSE.MD](./LICENSE.MD)

---

**© 2024 SIGREF - Sistema de Gestión de la Receptoría de Fondos**

*Manual Técnico - Versión 1.0.0 - Diciembre 2024*
