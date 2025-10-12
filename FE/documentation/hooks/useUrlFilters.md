## 🔗 Navegación  

⬅️ [Volver Atrás](./index.md)  
🏠 [Volver al Inicio](/documentation/index.md)  

# 📌 useUrlFilters

Hook personalizado para gestionar filtros, búsqueda y paginación mediante URL parameters. Permite persistir el estado de los filtros en la URL para compartir, refrescar y navegar manteniendo el contexto.

## API

### `useUrlFilters<T>(config)`

#### Parámetros

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `defaultValues` | `T extends Record<string, any>` | ✅ Sí | Objeto con los valores por defecto de todos los filtros |
| `serializers` | `Partial<Record<keyof T, (value: any) => string>>` | ❌ No | Funciones personalizadas para convertir valores a strings |
| `deserializers` | `Partial<Record<keyof T, (value: string) => any>>` | ❌ No | Funciones personalizadas para convertir strings a valores |

#### Retorna

```typescript
{
  filters: T,                                         // Objeto con los valores actuales
  setFilter: <K>(key: K, value: T[K]) => void,        // Actualiza un filtro
  setFilters: (updates: Partial<T>) => void,          // Actualiza múltiples filtros
  resetFilters: () => void,                           // Resetea todos los filtros
  resetSpecificFilters: (keys: (keyof T)[]) => void   // Resetea filtros específicos
}
```

### Ejemplo Simple

```typescript
function ProductsPage() {
  const { filters, setFilter } = useUrlFilters({
    defaultValues: {
      search: "",
      category: undefined,
      page: 1,
      pageSize: 10,
    }
  });

  return (
    <div>
      <input
        value={filters.search}
        onChange={(e) => setFilter("search", e.target.value)}
      />
      <p>Página actual: {filters.page}</p>
    </div>
  );
}
```

## Ejemplos Avanzados

### 1. Con Ant Design Table y Search

```typescript
import { Table, Input, Select } from "antd";
import { useUrlFilters } from "../hooks/useUrlFilters";

export function UsersPage() {
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      role: undefined as string | undefined,
      status: "active",
      page: 1,
      pageSize: 20,
    }
  });

  // Filtrar datos
  const filteredUsers = users.filter(user => {
    const matchesSearch = user.name.toLowerCase().includes(filters.search.toLowerCase());
    const matchesRole = !filters.role || user.role === filters.role;
    const matchesStatus = user.status === filters.status;
    return matchesSearch && matchesRole && matchesStatus;
  });

  return (
    <div>
      <Input.Search
        value={filters.search}
        onChange={(e) => setFilter("search", e.target.value)}
        placeholder="Buscar usuarios..."
      />
      
      <Select
        value={filters.role}
        onChange={(value) => setFilter("role", value)}
        options={[
          { label: "Admin", value: "admin" },
          { label: "Usuario", value: "user" },
        ]}
      />

      <Table
        dataSource={filteredUsers}
        pagination={{
          current: filters.page,
          pageSize: filters.pageSize,
          onChange: (page, pageSize) => setFilters({ page, pageSize }),
        }}
      />
    </div>
  );
}
```

### 2. Con Serializers y Deserializers Personalizados

Para tipos de datos complejos como fechas o arrays:

```typescript
const { filters, setFilter } = useUrlFilters({
  defaultValues: {
    startDate: new Date(),
    tags: [] as string[],
    priceRange: { min: 0, max: 1000 },
  },
  serializers: {
    startDate: (date) => date.toISOString(),
    tags: (tags) => tags.join(","),
    priceRange: (range) => `${range.min}-${range.max}`,
  },
  deserializers: {
    startDate: (str) => new Date(str),
    tags: (str) => str.split(","),
    priceRange: (str) => {
      const [min, max] = str.split("-").map(Number);
      return { min, max };
    },
  },
});

// Uso
setFilter("startDate", new Date("2025-01-01"));
setFilter("tags", ["react", "typescript"]);
setFilter("priceRange", { min: 100, max: 500 });
```

### 3. Resetear Filtros

```typescript
const { filters, resetFilters, resetSpecificFilters } = useUrlFilters({
  defaultValues: {
    search: "",
    category: undefined,
    minPrice: 0,
    maxPrice: 1000,
    page: 1,
  }
});

// Resetear todos los filtros
<button onClick={resetFilters}>
  Limpiar Filtros
</button>

// Resetear solo algunos filtros (mantiene paginación)
<button onClick={() => resetSpecificFilters(["search", "category"])}>
  Limpiar Búsqueda
</button>
```

## Casos de Uso Comunes

### Caso 1: Tabla con Búsqueda y Filtros

```typescript
const { filters, setFilter, setFilters } = useUrlFilters({
  defaultValues: {
    search: "",
    department: undefined,
    status: "active",
    page: 1,
    pageSize: 10,
  }
});

// URL resultante: ?search=hospital&department=emergency&page=2&pageSize=20
```

### Caso 2: Filtros de Rango

```typescript
const { filters, setFilters } = useUrlFilters({
  defaultValues: {
    minPrice: 0,
    maxPrice: 1000,
    minDate: "",
    maxDate: "",
  }
});

// Actualizar rango completo
setFilters({
  minPrice: 100,
  maxPrice: 500,
});
```

### Caso 3: Filtros con Valores Booleanos

```typescript
const { filters, setFilter } = useUrlFilters({
  defaultValues: {
    showInactive: false,
    includeArchived: false,
  }
});

// URL: ?showInactive=true&includeArchived=false
```

## Mejores Prácticas

### ✅ DO

1. **Usa tipos específicos para valores opcionales**
   ```typescript
   department: undefined as string | undefined  // ✅ Correcto
   ```

2. **Mantén los valores por defecto simples**
   ```typescript
   page: 1,
   pageSize: 10,
   search: "",
   ```

3. **Actualiza múltiples filtros relacionados juntos**
   ```typescript
   setFilters({ page: 1, pageSize: 20 });  // ✅ Una sola actualización
   ```

4. **Usa nombres descriptivos para los filtros**
   ```typescript
   searchQuery: "",      // ❌ Redundante
   search: "",           // ✅ Claro y conciso
   ```

### ❌ DON'T

1. **No uses valores por defecto complejos sin serializers**
   ```typescript
   // ❌ Sin serializer personalizado
   filters: { complex: { nested: { data: true } } }
   
   // ✅ Con serializer
   filters: { isActive: true }
   ```

2. **No actualices filtros individuales en loops**
   ```typescript
   // ❌ Múltiples actualizaciones
   filters.forEach(f => setFilter(f.key, f.value));
   
   // ✅ Una sola actualización
   setFilters(Object.fromEntries(filters.map(f => [f.key, f.value])));
   ```

3. **No dependas de la URL para lógica crítica**
   ```typescript
   // ❌ La URL puede ser manipulada por el usuario
   if (filters.isAdmin === true) {
     showAdminPanel();
   }
   
   // ✅ Valida en el backend
   if (user.role === "admin") {
     showAdminPanel();
   }
   ```

## Comportamiento de Deserialización Automática

El hook deserializa automáticamente según el tipo del valor por defecto:

| Tipo en `defaultValues` | Comportamiento |
|-------------------------|----------------|
| `string` | Mantiene como string |
| `number` | Convierte con `Number(value)` |
| `boolean` | Convierte `"true"` → `true`, resto → `false` |
| `undefined` | String o `undefined` si está vacío |
| Objeto/Array | Requiere deserializer personalizado |

### Ejemplo de Deserialización

```typescript
// defaultValues
{
  page: 1,            // number
  search: "",         // string
  active: true,       // boolean
  category: undefined // string | undefined
}

// URL: ?page=5&search=hospital&active=false&category=emergency

// filters resultante
{
  page: 5,              // ✅ Convertido a number
  search: "hospital",   // ✅ String
  active: false,        // ✅ Convertido a boolean
  category: "emergency" // ✅ String (no undefined porque tiene valor)
}
```
