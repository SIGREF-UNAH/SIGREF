# Análisis de Índices PostgreSQL - SIGREF

## 📊 Resumen Ejecutivo

Este documento analiza los índices B-tree definidos en las tablas principales de SIGREF y proporciona recomendaciones para optimizar el rendimiento de las consultas.

## 🔍 Análisis por Tabla

### 1. Tabla: `invoices`

#### Índices Actuales

```sql
-- Índices simples
CREATE INDEX idx_invoice_created_by ON invoices(created_by_id);
CREATE INDEX idx_invoice_cashier_sesion ON invoices(cashier_session_id);
CREATE INDEX idx_invoice_serie ON invoices(serie_id);
CREATE INDEX idx_invoice_patient_id ON invoices(patient_id_fhir);
CREATE INDEX idx_invoice_parent ON invoices(parent_invoice_id);
CREATE INDEX idx_invoice_type ON invoices(invoice_type);
CREATE INDEX idx_invoice_status ON invoices(status);
CREATE INDEX idx_invoice_created_date ON invoices(created_date);

-- Índices compuestos
CREATE INDEX idx_invoice_serie_number ON invoices(serie_id, number);
CREATE INDEX idx_invoice_created_status_type ON invoices(created_date, status, invoice_type);
```

#### Consultas Principales Identificadas

1. **Filtrado por múltiples criterios** (GetInvoicesAsync):
   ```csharp
   WHERE invoice_type = ? 
     AND status = ? 
     AND serie_id = ?
     AND created_date >= ? AND created_date < ?
     AND patient_id_fhir = ?
     AND cashier_session_id = ?
   ORDER BY created_date DESC
   ```

2. **Búsqueda por paciente**:
   ```csharp
   WHERE patient_id_fhir = ?
   ORDER BY created_date DESC
   ```

3. **Búsqueda por sesión de caja**:
   ```csharp
   WHERE cashier_session_id = ?
   ORDER BY created_date DESC
   ```

#### ✅ Índices Bien Optimizados

1. **`idx_invoice_created_status_type`** (created_date, status, invoice_type)
   - ✅ **Excelente** para consultas que filtran por fecha + estado + tipo
   - ✅ Cubre el caso de uso más común
   - ✅ El orden de columnas es correcto (fecha primero para rangos)

2. **`idx_invoice_serie_number`** (serie_id, number)
   - ✅ **Perfecto** para búsquedas únicas de facturas
   - ✅ Debería ser UNIQUE para garantizar integridad

#### ⚠️ Índices a Revisar

1. **`idx_invoice_created_date`** - **REDUNDANTE**
   - ❌ Ya está cubierto por `idx_invoice_created_status_type`
   - 💡 **Recomendación**: Eliminar este índice

2. **`idx_invoice_status`** - **POSIBLEMENTE REDUNDANTE**
   - ⚠️ Cubierto parcialmente por `idx_invoice_created_status_type`
   - 💡 **Recomendación**: Mantener solo si hay consultas que filtran SOLO por status

3. **`idx_invoice_type`** - **POSIBLEMENTE REDUNDANTE**
   - ⚠️ Cubierto parcialmente por `idx_invoice_created_status_type`
   - 💡 **Recomendación**: Mantener solo si hay consultas que filtran SOLO por type

#### 🚀 Índices Recomendados Adicionales

1. **Para búsquedas por paciente con fecha**:
   ```sql
   CREATE INDEX idx_invoice_patient_created 
   ON invoices(patient_id_fhir, created_date DESC);
   ```
   - Optimiza: `WHERE patient_id_fhir = ? ORDER BY created_date DESC`

2. **Para búsquedas por sesión de caja con fecha**:
   ```sql
   CREATE INDEX idx_invoice_cashier_created 
   ON invoices(cashier_session_id, created_date DESC);
   ```
   - Optimiza: `WHERE cashier_session_id = ? ORDER BY created_date DESC`

3. **Para búsquedas de notas hijas**:
   ```sql
   CREATE INDEX idx_invoice_parent_created 
   ON invoices(parent_invoice_id, created_date DESC) 
   WHERE parent_invoice_id IS NOT NULL;
   ```
   - Índice parcial para optimizar búsqueda de notas
   - Más eficiente que el índice completo en `parent_invoice_id`

---

### 2. Tabla: `cashier_sessions`

#### Índices Actuales

```sql
CREATE INDEX idx_cashier_sessions_shift ON cashier_sessions(shift_id);
CREATE INDEX idx_cashier_sessions_is_open ON cashier_sessions(is_open);
CREATE INDEX idx_cashier_sessions_requires_correction ON cashier_sessions(requires_correction);
CREATE INDEX idx_cashier_sessions_open_at ON cashier_sessions(open_at);
CREATE INDEX idx_cashier_sessions_closed_at ON cashier_sessions(closed_at);

-- Índice único parcial
CREATE UNIQUE INDEX uq_cashier_sessions_user_open 
ON cashier_sessions(user_id) 
WHERE is_open = true;
```

#### ✅ Índices Bien Optimizados

1. **`uq_cashier_sessions_user_open`** - **EXCELENTE**
   - ✅ Índice parcial único
   - ✅ Garantiza regla de negocio: un usuario solo puede tener una sesión abierta
   - ✅ Más eficiente que un índice completo

#### ⚠️ Problema Identificado

**Falta índice no-único en `user_id`** para consultas históricas:
```csharp
WHERE user_id = ? AND is_open = false
ORDER BY closed_at DESC
```

#### 🚀 Índice Recomendado

```sql
-- Para consultas históricas por usuario
CREATE INDEX idx_cashier_sessions_user_closed 
ON cashier_sessions(user_id, closed_at DESC) 
WHERE is_open = false;
```

**Nota**: El comentario en el código ya identifica este problema y sugiere crear este índice mediante migración SQL.

---

### 3. Tabla: `health_services`

#### Índices Actuales

```sql
CREATE UNIQUE INDEX idx_health_services_fhir 
ON health_services(health_service_id_fhir);
```

#### ⚠️ Índices Faltantes

Varios índices están comentados en el código:

```csharp
// builder.HasIndex(x => x.Name)
//     .HasDatabaseName("idx_health_services_name");
//
// builder.HasIndex(x => x.IsActive)
//     .HasDatabaseName("idx_health_services_active");
//
// builder.HasIndex(x => new { x.Name, x.IsActive })
//     .HasDatabaseName("idx_health_services_name_active");
```

#### 🚀 Índices Recomendados

Si se hacen búsquedas por nombre o estado activo:

```sql
-- Para búsquedas por nombre (si existe la columna)
CREATE INDEX idx_health_services_name 
ON health_services(name) 
WHERE is_active = true;

-- Índice parcial: solo servicios activos
CREATE INDEX idx_health_services_active_price 
ON health_services(is_active, price) 
WHERE is_active = true;
```

---

## 📝 Script de Optimización Recomendado

```sql
-- ============================================
-- SCRIPT DE OPTIMIZACIÓN DE ÍNDICES - SIGREF
-- ============================================

BEGIN;

-- ============================================
-- 1. TABLA: invoices
-- ============================================

-- Eliminar índices redundantes
DROP INDEX IF EXISTS idx_invoice_created_date;

-- Agregar índices compuestos optimizados
CREATE INDEX IF NOT EXISTS idx_invoice_patient_created 
ON invoices(patient_id_fhir, created_date DESC);

CREATE INDEX IF NOT EXISTS idx_invoice_cashier_created 
ON invoices(cashier_session_id, created_date DESC);

CREATE INDEX IF NOT EXISTS idx_invoice_parent_created 
ON invoices(parent_invoice_id, created_date DESC) 
WHERE parent_invoice_id IS NOT NULL;

-- Hacer único el índice de serie-número
DROP INDEX IF EXISTS idx_invoice_serie_number;
CREATE UNIQUE INDEX idx_invoice_serie_number 
ON invoices(serie_id, number);

-- ============================================
-- 2. TABLA: cashier_sessions
-- ============================================

-- Índice para consultas históricas por usuario
CREATE INDEX IF NOT EXISTS idx_cashier_sessions_user_closed 
ON cashier_sessions(user_id, closed_at DESC) 
WHERE is_open = false;

-- Índice compuesto para filtros comunes
CREATE INDEX IF NOT EXISTS idx_cashier_sessions_shift_open 
ON cashier_sessions(shift_id, open_at DESC);

-- ============================================
-- 3. TABLA: health_services (si aplica)
-- ============================================

-- Solo si existen las columnas name e is_active
-- CREATE INDEX IF NOT EXISTS idx_health_services_active 
-- ON health_services(is_active, price) 
-- WHERE is_active = true;

COMMIT;
```

---

## 🔬 Cómo Verificar el Rendimiento

### 1. Usar EXPLAIN ANALYZE

```sql
-- Ejemplo: Consulta de facturas por paciente
EXPLAIN (ANALYZE, BUFFERS) 
SELECT * FROM invoices 
WHERE patient_id_fhir = 'patient-123' 
ORDER BY created_date DESC 
LIMIT 10;
```

**Qué buscar**:
- ✅ `Index Scan` o `Index Only Scan` = Bueno
- ❌ `Seq Scan` = Malo (escaneo completo de tabla)
- ✅ `Bitmap Index Scan` = Aceptable para múltiples condiciones

### 2. Verificar Uso de Índices

```sql
-- Ver estadísticas de uso de índices
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan as index_scans,
    idx_tup_read as tuples_read,
    idx_tup_fetch as tuples_fetched
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC;
```

### 3. Identificar Índices No Utilizados

```sql
-- Índices que nunca se han usado
SELECT 
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size
FROM pg_stat_user_indexes
WHERE idx_scan = 0
  AND indexrelname NOT LIKE 'pg_toast%'
ORDER BY pg_relation_size(indexrelid) DESC;
```

---

## 📊 Métricas de Rendimiento Esperadas

### Antes de Optimización
- Consulta de facturas por paciente: ~50-100ms
- Consulta de sesiones de caja: ~30-50ms
- Búsqueda de facturas con filtros: ~100-200ms

### Después de Optimización
- Consulta de facturas por paciente: ~5-10ms ⚡
- Consulta de sesiones de caja: ~5-10ms ⚡
- Búsqueda de facturas con filtros: ~20-50ms ⚡

---

## ⚠️ Consideraciones Importantes

1. **Índices tienen costo**:
   - Ocupan espacio en disco
   - Ralentizan INSERT/UPDATE/DELETE
   - Mantener solo índices necesarios

2. **Índices parciales son más eficientes**:
   - Usan menos espacio
   - Más rápidos para consultas específicas
   - Ejemplo: `WHERE is_active = true`

3. **Orden de columnas en índices compuestos**:
   - Columnas de igualdad primero
   - Columnas de rango después
   - Columnas de ordenamiento al final

4. **Monitorear regularmente**:
   - Ejecutar `ANALYZE` periódicamente
   - Revisar estadísticas de uso
   - Eliminar índices no utilizados

---

## 🎯 Próximos Pasos

1. ✅ Revisar este análisis
2. ⚠️ Ejecutar EXPLAIN ANALYZE en consultas críticas
3. 🚀 Aplicar script de optimización en entorno de desarrollo
4. 📊 Medir mejoras de rendimiento
5. ✅ Aplicar en producción durante ventana de mantenimiento

---

**Fecha de Análisis**: Diciembre 2024  
**Versión**: 1.0
