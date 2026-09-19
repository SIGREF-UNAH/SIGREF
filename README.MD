# SIGREF

Sistema de gestión administrativa para centralizar operaciones de pacientes, servicios, caja, ingresos, facturación, reportes y auditoría en organizaciones de salud.

SIGREF combina una aplicación web, una API y servicios de soporte para reducir tareas manuales, ordenar la información operativa y ofrecer trazabilidad sobre las operaciones del sistema.

> **Software propietario:** este repositorio no es un proyecto de código abierto. El código, la arquitectura y la documentación están protegidos por la [licencia propietaria](./LICENSE). También se incluye una [versión en español](./LICENSE_ES.md).

## Qué resuelve

SIGREF ayuda a organizar en un solo sistema:

- Registro y consulta de pacientes y profesionales.
- Catálogos de servicios, ubicaciones, organizaciones y valores.
- Apertura, operación y cierre de sesiones de caja.
- Series, facturas, pagos e ingresos administrativos.
- Dashboards y reportes operativos y financieros.
- Auditoría de eventos y trazabilidad de cambios.
- Gestión de usuarios, roles y permisos.
- Integración de información clínica de referencia mediante HL7 FHIR.
- Procesamiento de tareas y generación de reportes en segundo plano.

El sistema está orientado a apoyar la operación administrativa. La instalación, configuración, seguridad de los datos y cumplimiento normativo deben ser evaluados por cada organización antes de utilizarlo en producción.

## Cómo ayuda

- Centraliza información que normalmente se gestiona en herramientas separadas.
- Reduce duplicidad de registros y facilita el seguimiento de cada operación.
- Mejora la visibilidad de ingresos, caja, servicios y actividad del sistema.
- Separa interfaz, API, persistencia, autenticación y tareas de fondo.
- Facilita la integración con servicios externos mediante API REST y modelos interoperables.
- Proporciona una base técnica para despliegues reproducibles con contenedores.

## Funcionalidades principales

| Área | Capacidades |
| --- | --- |
| Pacientes y profesionales | Consulta, registro y relación con recursos FHIR. |
| Servicios y catálogos | Servicios de salud, grupos, ubicaciones, organizaciones y valores. |
| Caja | Sesiones, turnos, movimientos y conciliación operativa. |
| Ingresos | Facturas, series, pagos y consultas por filtros. |
| Reportes | Dashboards, métricas, exportaciones y trabajos en segundo plano. |
| Auditoría | Historial de eventos y trazabilidad de operaciones. |
| Administración | Usuarios, roles, permisos, archivos y configuración. |

## Arquitectura resumida

```text
Frontend React/TypeScript
        │ REST / OpenAPI
        ▼
ASP.NET Core API ─── Keycloak (OIDC/JWT/RBAC)
        │
        ├── PostgreSQL + Entity Framework Core
        ├── MongoDB para auditoría y datos documentales
        ├── Servidor HL7 FHIR
        └── Hangfire Worker para tareas y reportes
```

## Tecnologías utilizadas

### Frontend

- React 19 y TypeScript.
- Vite como herramienta de desarrollo y construcción.
- Ant Design y Tailwind CSS para la interfaz.
- TanStack Query y Axios para acceso y caché de datos.
- Zustand para estado local.
- Formik y Yup para formularios y validación.
- React Router para navegación.
- Orval para generar el cliente TypeScript desde OpenAPI.
- Vitest, ESLint y Prettier para pruebas y calidad del código.

### Backend

- .NET 9 y ASP.NET Core Web API.
- Entity Framework Core con PostgreSQL.
- MongoDB para auditoría y almacenamiento documental.
- .NET Aspire para composición de servicios y configuración compartida.
- Hangfire y un worker dedicado para procesamiento asíncrono.
- Keycloak para identidad, autenticación y autorización.
- OpenAPI para documentar y consumir la API.
- HL7 FHIR R4 mediante la librería `Hl7.Fhir.R4`.
- OpenTelemetry para métricas, trazas y telemetría.

### Infraestructura

- Docker y Docker Compose.
- Nginx como proxy y servidor del frontend.
- PostgreSQL, MongoDB, Keycloak, HAPI FHIR y Seq.
- OpenTelemetry Collector para observabilidad.

## Estándares y prácticas

- API REST con contratos OpenAPI.
- HL7 FHIR R4 para interoperabilidad de información clínica.
- OpenID Connect y JWT Bearer para autenticación.
- RBAC para autorización basada en roles.
- Entity Framework Core para acceso tipado a datos relacionales.
- Validación de entradas, separación por capas y DTOs para contratos de API.
- Pruebas automatizadas en frontend y backend.
- ESLint, Prettier y convenciones de .NET para calidad y consistencia.
- Docker Compose para entornos reproducibles.
- OpenTelemetry para observabilidad.

## Estructura del repositorio

```text
SIGREF/
├── BE/                  # Solución .NET, API, dominio, persistencia y worker
├── FE/                  # Aplicación React/TypeScript
├── config/              # Configuración de bases de datos y servicios
├── nginx/               # Configuración del proxy/frontend
├── licenses/            # Información sobre dependencias y software de terceros
├── docker-compose.yml   # Orquestación local de servicios
├── LICENSE              # Licencia propietaria en inglés
├── LICENSE_ES.md        # Licencia propietaria en español
└── README.MD            # Este documento
```

## Requisitos

- Git.
- Docker Desktop con Docker Compose para el entorno completo.
- Node.js 22 para el frontend.
- .NET 9 SDK para el backend.

## Desarrollo local

### Frontend

```bash
cd FE
npm ci
npm run dev
```

Comandos disponibles:

```bash
npm run lint
npm run typecheck
npm run test:run
npm run build
```

### Backend

```bash
dotnet restore BE/SIGREF.API.sln
dotnet build BE/SIGREF.API.sln
dotnet test BE/SIGREF.API.sln
```

Para levantar el entorno completo, utiliza la configuración de Docker Compose del repositorio y las variables de entorno definidas para cada instalación.

## Documentación

- [Documentación general](./docs/).
- [Documentación del frontend](./FE/documentation/).
- [Licencias y software de terceros](./licenses/THIRD_PARTY.md).

## Autores

El desarrollo registrado del proyecto incluye a:

- [Anthony Edward Miranda Fuentes](https://github.com/AnthonyEMF)
- [Carlos Ovidio Dubon Pineda](https://github.com/Pineda04)
- [Danilo Isaac Vides Chicas](https://github.com/IsaacV04)
- [Erick Marley Arita](https://github.com/erickArita)
- [Ever Josue Garcia Leonor](https://github.com/everjosue56)
- [Hector Rene Martinez Vega](https://github.com/TETvega)
- [Josue David Diaz Rodriguez](https://github.com/JDDR200530)
- [Michael Andrey Galdamez Martinez](https://github.com/MichaelGald)

## Uso de terceros

SIGREF utiliza frameworks, librerías, herramientas, imágenes y servicios de terceros. Sus respectivas marcas, avisos y licencias pertenecen a sus titulares y no quedan sustituidos por la licencia propietaria de SIGREF.

Antes de redistribuir una instalación, imagen o producto que incluya componentes externos, revisa sus condiciones y conserva los avisos legales correspondientes. Consulta el inventario y las reglas en [licenses/THIRD_PARTY.md](./licenses/THIRD_PARTY.md).

## Licencia

El código propio de SIGREF se distribuye bajo una licencia propietaria y restrictiva:

- [English license](./LICENSE)
- [Licencia en español](./LICENSE_ES.md)

No se concede permiso para copiar, distribuir, modificar, crear forks, reutilizar la arquitectura o crear productos derivados sin autorización escrita de los titulares de los derechos.
