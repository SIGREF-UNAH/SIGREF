# Migración de Swagger a Scalar en SIGREF.API

## Objetivo

Reemplazar la interfaz y el pipeline de documentación Swagger de `SIGREF.API` por Scalar, manteniendo el comportamiento funcional actual de la documentación en desarrollo y eliminando dependencias, configuración y código específico de Swashbuckle.

## Alcance

- Scalar estará disponible únicamente cuando el entorno sea `Development`, igual que Swagger actualmente.
- La documentación conservará los endpoints, agrupaciones, descripciones XML, respuestas comunes y esquema de autenticación JWT existentes.
- La interfaz pública de documentación será `/scalar`.
- Las rutas y middleware de `/swagger` se eliminarán.
- Se eliminarán las dependencias `Swashbuckle.*` y el código que exista únicamente para Swashbuckle.
- Los atributos de documentación se migrarán a mecanismos compatibles con OpenAPI nativo cuando sea necesario, sin alterar el comportamiento de los endpoints.

## Diseño técnico

La aplicación usará la generación OpenAPI integrada en ASP.NET Core (`AddOpenApi`/`MapOpenApi`) como fuente del documento JSON. Scalar se mapeará sobre ese documento mediante `Scalar.AspNetCore`, con una ruta estable `/scalar` y el esquema Bearer/JWT configurado para permitir autorización desde la interfaz.

La configuración de servicios seguirá separada en `Startup.MvcSwagger.cs` o en un archivo renombrado a una responsabilidad neutral de documentación API. La configuración del pipeline eliminará el bloque de depuración y las llamadas `UseSwagger`/`UseSwaggerUI`, y añadirá el mapeo de Scalar solo en desarrollo.

La documentación existente se conservará usando XML comments y atributos de OpenAPI/ASP.NET Core. El filtro de enums se eliminará si deja de ser necesario con la generación nativa; si el formato camelCase requiere una transformación equivalente, se implementará con la extensión oficial disponible para OpenAPI nativo.

## Limpieza

Se eliminarán:

- Paquetes `Swashbuckle.AspNetCore`, `Swashbuckle.AspNetCore.Annotations`, `Swashbuckle.AspNetCore.Filters` y `Swashbuckle.AspNetCore.SwaggerUI`.
- `using` y atributos de Swashbuckle en controladores, DTOs y enums.
- `EnumSchemaFilter` y cualquier configuración exclusiva de `ISchemaFilter`.
- El middleware temporal de depuración de Swagger.
- `launchUrl` con valor `swagger`.
- Comentarios y exclusiones de auditoría específicas de `/swagger`.

## Validación

- Compilar la solución backend.
- Ejecutar las pruebas existentes aplicables.
- Confirmar que `/scalar` y el documento OpenAPI responden en Development.
- Confirmar que no existen referencias de código a `Swagger`, `Swashbuckle` o `/swagger` fuera de documentación histórica justificada.
- Revisar que el proyecto ya no restaure paquetes Swashbuckle.
