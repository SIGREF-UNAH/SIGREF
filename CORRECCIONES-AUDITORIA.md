# Correcciones al Sistema de Auditoría

## Problema Identificado

Las acciones de auditoría (create, update, delete, read) no se estaban registrando correctamente. Por ejemplo:
- Un PUT podía registrarse como "update" incluso cuando creaba un recurso nuevo (código 201)
- Un POST podía registrarse como "create" incluso cuando actualizaba (código 200)

## Solución Implementada

### 1. Middleware de Auditoría Corregido

**Archivo**: `BE/SIGREF.API/Audit/Middleware/AuditMiddleware.cs`

**Cambio Principal**: Ahora la acción se determina basándose en el **código de estado HTTP** de la respuesta, no solo en el método HTTP:

```csharp
private string MapHttpMethodToAction(string httpMethod, int statusCode)
{
    return httpMethod switch
    {
        "POST" => statusCode == 201 ? "create" : "update",
        "PUT" => statusCode == 201 ? "create" : "update",
        "PATCH" => "update",
        "DELETE" => "delete",
        "GET" => "read",
        _ => "unknown"
    };
}
```

**Lógica**:
- **POST con 201**: create (recurso nuevo creado)
- **POST con 200**: update (recurso actualizado)
- **PUT con 201**: create (recurso nuevo creado)
- **PUT con 200**: update (recurso actualizado)
- **PATCH**: siempre update
- **DELETE**: siempre delete
- **GET**: siempre read

### 2. Campo UserName Agregado

El modelo `AuditLog` ahora incluye el campo `UserName` para facilitar búsquedas:

```csharp
[BsonElement("userName")]
public string UserName { get; set; }
```

### 3. Controlador Actualizado

**Archivo**: `BE/SIGREF.API/Controllers/Audit/AuditController.cs`

Ahora permite buscar por:
- `userId`: ID del usuario
- `userName`: Nombre de usuario (username)
- `action`: Tipo de acción (create, update, delete, read)
- `from` y `to`: Rango de fechas

## Próximos Pasos Necesarios

### Backend

1. **Actualizar IAuditService.cs** para devolver tuplas con totalCount
2. **Actualizar AuditService.cs** para implementar los nuevos métodos
3. **Agregar método GetLogsByUserNameAsync** en el servicio

### Frontend

1. **Revisar componentes de auditoría** para usar los filtros correctos
2. **Actualizar llamadas a la API** para incluir los nuevos parámetros
3. **Regenerar cliente API** con Orval después de los cambios

## Comandos para Aplicar Cambios

```bash
# Backend
cd BE/SIGREF.API
dotnet build

# Frontend
cd FE
npm run orval  # Regenerar cliente API
```

## Testing

Probar los siguientes escenarios:

1. **Crear un paciente** (POST) → Debe registrarse como "create"
2. **Actualizar un paciente** (PUT) → Debe registrarse como "update"
3. **Eliminar un paciente** (DELETE) → Debe registrarse como "delete"
4. **Consultar un paciente** (GET) → Debe registrarse como "read"
5. **Filtrar por action="create"** → Solo debe traer creaciones
6. **Filtrar por action="update"** → Solo debe traer actualizaciones
7. **Filtrar por userName** → Debe traer logs de ese usuario específico
