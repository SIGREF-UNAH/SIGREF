# Software y licencias de terceros

SIGREF es software propietario. Esta carpeta documenta la relación entre la licencia del proyecto y las dependencias externas utilizadas por sus componentes.

## Regla principal

La licencia propietaria de SIGREF solo cubre el código, la documentación y los materiales originales del proyecto. No reemplaza, altera ni amplía las licencias de los frameworks, paquetes, imágenes, contenedores o servicios de terceros.

Cada distribución debe conservar los avisos y condiciones que correspondan a los componentes incluidos. El responsable de una distribución debe revisar las versiones concretas y sus avisos antes de publicar binarios, imágenes o servicios derivados.

## Fuentes de inventario

Las dependencias declaradas se mantienen en los archivos fuente del proyecto:

- Frontend: [`FE/package.json`](../FE/package.json) y [`FE/package-lock.json`](../FE/package-lock.json).
- Backend: archivos [`*.csproj`](../BE/) y [`BE/SIGREF.API.sln`](../BE/SIGREF.API.sln).
- Infraestructura: [`docker-compose.yml`](../docker-compose.yml), Dockerfiles y configuraciones de `nginx/`.

## Componentes principales

| Componente | Uso en SIGREF | Fuente de referencia |
| --- | --- | --- |
| React, TypeScript y Vite | Interfaz web y construcción del frontend. | `FE/package.json` |
| ASP.NET Core y .NET | API, servicios y worker. | `BE/**/*.csproj` |
| Entity Framework Core y PostgreSQL | Persistencia relacional. | `BE/SIGREF.Infrastructure.Persistence/` |
| MongoDB | Auditoría y datos documentales. | `docker-compose.yml` y backend |
| Keycloak | Identidad, OIDC, JWT y roles. | `docker-compose.yml` y backend |
| HAPI FHIR / HL7 FHIR | Interoperabilidad de recursos clínicos. | `docker-compose.yml` y backend |
| Hangfire | Trabajos y procesamiento asíncrono. | `BE/SIGREF.Hangfire.Worker/` |
| Docker, Nginx y OpenTelemetry | Despliegue, proxy y observabilidad. | infraestructura del repositorio |

## Responsabilidad del distribuidor

Antes de redistribuir una instalación o imagen:

1. Genera un inventario de dependencias de la versión exacta.
2. Revisa las licencias y avisos de cada paquete y de cada imagen base.
3. Conserva los avisos requeridos por terceros.
4. No presentes componentes de terceros como propiedad de SIGREF.
5. Respeta simultáneamente la licencia propietaria de SIGREF y las licencias de sus dependencias.

La inclusión de una dependencia de terceros no concede permiso para reutilizar el código propio de SIGREF ni su arquitectura.
