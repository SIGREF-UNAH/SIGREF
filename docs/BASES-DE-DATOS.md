# 🗄️ Esquema y Guía de Bases de Datos

## Visión General

SIGREF utiliza **dos sistemas de bases de datos** para optimizar el almacenamiento y consulta de datos:

- **PostgreSQL 17** → Datos transaccionales y relacionales (OLTP)
- **MongoDB 5.0+** → Logs, auditoría y datos no estructurados (Document Store)

---

## 📊 PostgreSQL - Base de Datos Relacional

PostgreSQL es la base de datos principal que almacena todos los datos operacionales de SIGREF.

### Tablas Principales

#### **1. Hospital Properties** (Configuración Institucional)

```sql
CREATE TABLE hospital_properties (
    id SERIAL PRIMARY KEY,
    organization_name VARCHAR(255) NOT NULL,
    hospital_name VARCHAR(255) NOT NULL,
    address VARCHAR(500),
    phone VARCHAR(20),
    email VARCHAR(120),
    legal_representative VARCHAR(255),
    nit VARCHAR(50),
    currency VARCHAR(3) DEFAULT 'HNL',
    timezone VARCHAR(50) DEFAULT 'America/Tegucigalpa',
    logo_url TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Propósito:** Almacenar configuración global del hospital/institución.

---

#### **2. Patients (FHIR-Compliant)**

```sql
CREATE TABLE patients (
    id SERIAL PRIMARY KEY,
    fhir_resource_id VARCHAR(255),  -- Referencia a servidor FHIR
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    date_of_birth DATE,
    gender VARCHAR(10),  -- male, female, other
    national_id VARCHAR(50),  -- ID card/Cédula
    email VARCHAR(120),
    phone VARCHAR(20),
    address TEXT,
    marital_status VARCHAR(20),
    race_ethnicity VARCHAR(100),
    religion VARCHAR(100),
    notes TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER,  -- User ID
    
    CONSTRAINT fk_patients_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id)
);

CREATE INDEX idx_patients_national_id ON patients(national_id);
CREATE INDEX idx_patients_email ON patients(email);
```

**Propósito:** Almacenar información demográfica de pacientes conforme a FHIR Patient resource.

---

#### **3. Practitioners** (Profesionales Sanitarios)

```sql
CREATE TABLE practitioners (
    id SERIAL PRIMARY KEY,
    fhir_resource_id VARCHAR(255),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    license_number VARCHAR(100) UNIQUE,
    speciality VARCHAR(255),
    phone VARCHAR(20),
    email VARCHAR(120),
    npi VARCHAR(50),  -- National Provider Identifier
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_practitioners_license ON practitioners(license_number);
```

**Propósito:** Gestionar profesionales sanitarios (médicos, enfermeras, etc.)

---

#### **4. Organizations** (Entidades Organizacionales)

```sql
CREATE TABLE organizations (
    id SERIAL PRIMARY KEY,
    fhir_resource_id VARCHAR(255),
    name VARCHAR(255) NOT NULL,
    type VARCHAR(100),  -- hospital, clinic, department
    parent_org_id INTEGER,
    address TEXT,
    phone VARCHAR(20),
    email VARCHAR(120),
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_organizations_parent FOREIGN KEY (parent_org_id) 
        REFERENCES organizations(id)
);

CREATE INDEX idx_organizations_name ON organizations(name);
```

**Propósito:** Definir estructura organizacional (departamentos, sucursales, etc.)

---

#### **5. Locations** (Ubicaciones Físicas)

```sql
CREATE TABLE locations (
    id SERIAL PRIMARY KEY,
    fhir_resource_id VARCHAR(255),
    name VARCHAR(255) NOT NULL,
    location_type VARCHAR(100),  -- room, ward, building
    address TEXT,
    organization_id INTEGER,
    operational_status VARCHAR(50),  -- active, inactive, suspended
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_locations_org FOREIGN KEY (organization_id) 
        REFERENCES organizations(id)
);
```

**Propósito:** Definir ubicaciones físicas (salas, consultorios, pisos, etc.)

---

#### **6. Health Services** (Catálogo de Servicios)

```sql
CREATE TABLE health_services (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    code VARCHAR(50),
    cost DECIMAL(18,2),
    duration_minutes INTEGER,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_services_code ON health_services(code);
```

**Propósito:** Almacenar servicios disponibles (consultas, análisis, tratamientos).

---

#### **7. Service Groups** (Grupos de Servicios)

```sql
CREATE TABLE service_groups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Propósito:** Agrupar servicios por categoría (laboratorio, radiología, etc.)

---

#### **8. Invoices** (Facturas)

```sql
CREATE TABLE invoices (
    id SERIAL PRIMARY KEY,
    invoice_number VARCHAR(50) UNIQUE NOT NULL,
    patient_id INTEGER NOT NULL,
    invoice_date DATE DEFAULT CURRENT_DATE,
    total_amount DECIMAL(18,2) NOT NULL,
    discount DECIMAL(18,2) DEFAULT 0,
    tax DECIMAL(18,2) DEFAULT 0,
    net_amount DECIMAL(18,2) NOT NULL,
    payment_status VARCHAR(20),  -- pending, partial, paid, cancelled
    payment_method VARCHAR(50),  -- cash, card, check, transfer
    due_date DATE,
    notes TEXT,
    created_by INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by INTEGER,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_invoices_patient FOREIGN KEY (patient_id) 
        REFERENCES patients(id),
    CONSTRAINT fk_invoices_created_by FOREIGN KEY (created_by) 
        REFERENCES users(id),
    CONSTRAINT fk_invoices_updated_by FOREIGN KEY (updated_by) 
        REFERENCES users(id)
);

CREATE INDEX idx_invoices_patient ON invoices(patient_id);
CREATE INDEX idx_invoices_date ON invoices(invoice_date);
CREATE INDEX idx_invoices_status ON invoices(payment_status);
```

**Propósito:** Almacenar facturas emitidas.

---

#### **9. Invoice Items** (Detalles de Factura)

```sql
CREATE TABLE invoice_items (
    id SERIAL PRIMARY KEY,
    invoice_id INTEGER NOT NULL,
    service_id INTEGER,
    description VARCHAR(500),
    quantity INTEGER DEFAULT 1,
    unit_price DECIMAL(18,2),
    total_price DECIMAL(18,2),
    notes TEXT,
    
    CONSTRAINT fk_items_invoice FOREIGN KEY (invoice_id) 
        REFERENCES invoices(id) ON DELETE CASCADE,
    CONSTRAINT fk_items_service FOREIGN KEY (service_id) 
        REFERENCES health_services(id)
);

CREATE INDEX idx_items_invoice ON invoice_items(invoice_id);
```

**Propósito:** Almacenar líneas detalladas de facturas.

---

#### **10. Invoice Series** (Series de Numeración)

```sql
CREATE TABLE invoice_series (
    id SERIAL PRIMARY KEY,
    series_code VARCHAR(10) UNIQUE NOT NULL,
    organization_id INTEGER,
    next_number INTEGER DEFAULT 1,
    last_used_number INTEGER,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_series_org FOREIGN KEY (organization_id) 
        REFERENCES organizations(id)
);
```

**Propósito:** Controlar series y correlativas de facturas.

---

#### **11. Cashier Sessions** (Sesiones de Caja)

```sql
CREATE TABLE cashier_sessions (
    id SERIAL PRIMARY KEY,
    cashier_id INTEGER NOT NULL,
    session_date DATE DEFAULT CURRENT_DATE,
    opening_balance DECIMAL(18,2),
    closing_balance DECIMAL(18,2),
    expected_amount DECIMAL(18,2),
    actual_amount DECIMAL(18,2),
    difference DECIMAL(18,2),
    status VARCHAR(20),  -- open, closed, reconciled
    notes TEXT,
    opened_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    closed_at TIMESTAMP,
    
    CONSTRAINT fk_sessions_cashier FOREIGN KEY (cashier_id) 
        REFERENCES users(id)
);

CREATE INDEX idx_sessions_cashier ON cashier_sessions(cashier_id);
CREATE INDEX idx_sessions_date ON cashier_sessions(session_date);
```

**Propósito:** Registrar sesiones de pago y cuadraturas de caja.

---

#### **12. Shifts** (Turnos de Trabajo)

```sql
CREATE TABLE shifts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    shift_date DATE DEFAULT CURRENT_DATE,
    start_time TIME,
    end_time TIME,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_shifts_user FOREIGN KEY (user_id) 
        REFERENCES users(id)
);

CREATE INDEX idx_shifts_user ON shifts(user_id);
CREATE INDEX idx_shifts_date ON shifts(shift_date);
```

**Propósito:** Gestionar turnos de trabajo de empleados.

---

#### **13. Media Files** (Documentos y Archivos)

```sql
CREATE TABLE media_files (
    id SERIAL PRIMARY KEY,
    file_name VARCHAR(255) NOT NULL,
    file_path TEXT NOT NULL,
    file_type VARCHAR(50),
    file_size BIGINT,
    uploaded_by INTEGER,
    entity_type VARCHAR(100),  -- Patient, Invoice, etc.
    entity_id INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_files_uploader FOREIGN KEY (uploaded_by) 
        REFERENCES users(id)
);

CREATE INDEX idx_files_entity ON media_files(entity_type, entity_id);
```

**Propósito:** Almacenar referencias a archivos (logs, documentos PDF, etc.)

---

#### **14. Users** (Usuarios del Sistema)

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    keycloak_id VARCHAR(255) UNIQUE,
    username VARCHAR(100) UNIQUE NOT NULL,
    email VARCHAR(120) UNIQUE NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    role VARCHAR(50),  -- admin, cashier, auditor, ti
    is_active BOOLEAN DEFAULT true,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_keycloak ON users(keycloak_id);
CREATE INDEX idx_users_email ON users(email);
```

**Propósito:** Sincronizar usuarios desde Keycloak localmente.

---

#### **15. Dashboard Facts** (Vista Materializada para Reportes)

```sql
CREATE TABLE dashboard_facts (
    id SERIAL PRIMARY KEY,
    fact_date DATE,
    total_invoices BIGINT DEFAULT 0,
    total_amount DECIMAL(18,2) DEFAULT 0,
    total_patients INTEGER DEFAULT 0,
    total_services INTEGER DEFAULT 0,
    payment_by_method JSONB,  -- { cash: 1000, card: 500, ... }
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_fact_date UNIQUE (fact_date)
);

CREATE INDEX idx_dashboard_date ON dashboard_facts(fact_date);
```

**Propósito:** Almacenar datos preaggregados para dashboards (mejor performance).

---

### Relaciones de Claves Foráneas

```
                    organizations (parent)
                           ▲
                           │
    health_services        │        locations
          │                │           │
          └────────┬───────┴───────┬───┘
                   │               │
                   └─ invoice_items─ invoices
                                      │
                                      └─ patients
                                      
    practitioners ─ practitioner_roles
          │                 │
          └─────────────────┘
          
    cashier_sessions ─ users
    shifts ─────────── users
    invoices ──────── users (created_by, updated_by)
```

---

## 🔐 MongoDB - Base de Datos de Logs y Auditoría

MongoDB almacena datos no estructurados y de acceso frecuente como logs y auditoría.

### Colecciones Principales

#### **1. audit_logs** (Logs de Auditoría Inmutables)

```javascript
db.createCollection("audit_logs", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["userId", "action", "timestamp"],
            properties: {
                _id: { bsonType: "objectId" },
                userId: { bsonType: "string", description: "User ID from Keycloak" },
                userEmail: { bsonType: "string" },
                action: { 
                    bsonType: "string",
                    description: "Action performed (CREATE, UPDATE, DELETE, etc)"
                },
                entityType: { bsonType: "string", description: "Entity affected (Invoice, Patient, etc)" },
                entityId: { bsonType: "int" },
                oldValues: { bsonType: "object", description: "Previous values (if UPDATE)" },
                newValues: { bsonType: "object", description: "New values" },
                timestamp: { bsonType: "date" },
                ipAddress: { bsonType: "string" },
                result: { bsonType: "string", enum: ["SUCCESS", "FAILURE", "DENIED"] },
                errorMessage: { bsonType: "string" },
                duration_ms: { bsonType: "int" }
            }
        }
    }
});

// Crear índices
db.audit_logs.createIndex({ userId: 1, timestamp: -1 });
db.audit_logs.createIndex({ entityType: 1, entityId: 1 });
db.audit_logs.createIndex({ timestamp: -1 });
db.audit_logs.createIndex({ action: 1 });

// TTL - Eliminar automáticamente después de 2 años
db.audit_logs.createIndex({ timestamp: 1 }, { expireAfterSeconds: 63072000 });
```

**Ejemplo de documento:**

```json
{
    "_id": ObjectId("65b1234567890abcdef01234"),
    "userId": "user-123",
    "userEmail": "cashier@hospital.hn",
    "action": "CREATE",
    "entityType": "Invoice",
    "entityId": 456,
    "oldValues": null,
    "newValues": {
        "id": 456,
        "invoiceNumber": "FAC-2026-001",
        "patientId": 789,
        "amount": 1500.00,
        "status": "paid"
    },
    "timestamp": ISODate("2026-01-31T14:30:00.000Z"),
    "ipAddress": "192.168.1.100",
    "result": "SUCCESS",
    "duration_ms": 245
}
```

---

#### **2. system_logs** (Logs de Sistema)

```javascript
db.createCollection("system_logs", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            properties: {
                _id: { bsonType: "objectId" },
                level: { bsonType: "string", enum: ["DEBUG", "INFO", "WARN", "ERROR", "FATAL"] },
                logger: { bsonType: "string" },
                message: { bsonType: "string" },
                exception: { bsonType: "string" },
                stackTrace: { bsonType: "string" },
                properties: { bsonType: "object" },
                timestamp: { bsonType: "date" }
            }
        }
    }
});

db.system_logs.createIndex({ timestamp: -1 });
db.system_logs.createIndex({ level: 1 });
db.system_logs.createIndex({ timestamp: 1 }, { expireAfterSeconds: 2592000 }); // 30 days
```

---

#### **3. user_sessions** (Sesiones de Usuario)

```javascript
db.createCollection("user_sessions", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            properties: {
                _id: { bsonType: "objectId" },
                userId: { bsonType: "string" },
                jti: { bsonType: "string", description: "JWT ID token" },
                loginTime: { bsonType: "date" },
                lastActivity: { bsonType: "date" },
                ipAddress: { bsonType: "string" },
                userAgent: { bsonType: "string" },
                isActive: { bsonType: "bool" }
            }
        }
    }
});

db.user_sessions.createIndex({ userId: 1, loginTime: -1 });
db.user_sessions.createIndex({ loginTime: 1 }, { expireAfterSeconds: 604800 }); // 7 days
```

---

#### **4. error_logs** (Registro de Errores)

```javascript
db.createCollection("error_logs", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            properties: {
                _id: { bsonType: "objectId" },
                errorCode: { bsonType: "string" },
                message: { bsonType: "string" },
                stackTrace: { bsonType: "string" },
                context: { bsonType: "object" },
                userId: { bsonType: "string" },
                endpoint: { bsonType: "string" },
                method: { bsonType: "string" },
                statusCode: { bsonType: "int" },
                timestamp: { bsonType: "date" }
            }
        }
    }
});

db.error_logs.createIndex({ timestamp: -1 });
db.error_logs.createIndex({ errorCode: 1 });
```

---

## 🔄 Sincronización PostgreSQL ↔ MongoDB

### Flujo de Auditoría

```
1. Usuario ejecuta acción en Frontend
   ↓
2. Backend recibe request
   ↓
3. Ejecuta lógica en PostgreSQL (transacción)
   ↓
4. Si exitosa, registra en MongoDB
   └─ await auditService.LogAsync(...)
   └─ db.audit_logs.insertOne({...})
   ↓
5. Retorna respuesta al Frontend
```

**Código de ejemplo:**

```csharp
public class AuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _auditCollection;
    
    public async Task LogAsync(AuditLog log)
    {
        log.Timestamp = DateTime.UtcNow;
        log.IpAddress = GetCurrentIpAddress();
        
        await _auditCollection.InsertOneAsync(log);
    }
}
```

---

## 📊 Vistas Materializadas

Para optimizar reportes, se crean vistas pre-agregadas que se actualizan periódicamente:

```sql
-- Vista: Ingresos por día
CREATE MATERIALIZED VIEW v_daily_revenue AS
SELECT 
    DATE(invoice_date) as date,
    COUNT(DISTINCT id) as total_invoices,
    SUM(net_amount) as total_revenue,
    COUNT(DISTINCT patient_id) as total_patients,
    payment_method,
    COUNT(*) as count_by_method
FROM invoices
WHERE payment_status = 'paid'
GROUP BY DATE(invoice_date), payment_method;

CREATE INDEX idx_v_daily_revenue_date ON v_daily_revenue(date);

-- Actualizar cada hora
-- (Usando cron job o stored procedure)
REFRESH MATERIALIZED VIEW CONCURRENTLY v_daily_revenue;
```

---

## 🔐 Seguridad de Datos

### Backup & Recovery

**PostgreSQL:**
```bash
# Backup full
pg_dump -U sigref_user sigref > backup.sql

# Restore
psql -U sigref_user sigref < backup.sql
```

**MongoDB:**
```bash
# Backup
mongodump --uri "mongodb://localhost:27017/sigref_logs" --out ./backup

# Restore
mongorestore --uri "mongodb://localhost:27017" ./backup
```

### Encriptación en Tránsito

- Usar SSL/TLS para conexiones PostgreSQL
- Usar SSL/TLS para conexiones MongoDB

### Encriptación en Reposo

- Habilitar encryption-at-rest en PostgreSQL
- Habilitar encryption-at-rest en MongoDB Enterprise

---

## 📈 Mejora de Performance

### Índices Críticos

```sql
-- Ya creados en las tablas
-- Verificar regularmente:
SELECT 
    schemaname,
    tablename,
    indexname
FROM pg_indexes
ORDER BY schemaname, tablename;
```

### Query Analysis

```sql
-- EXPLAIN para analizar planes de ejecución
EXPLAIN ANALYZE
SELECT i.*, p.full_name
FROM invoices i
JOIN patients p ON i.patient_id = p.id
WHERE i.invoice_date = CURRENT_DATE;
```

---

**Última actualización:** 31 de enero de 2026
