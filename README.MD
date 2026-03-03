# 🏥 SIGREF - Sistema de Gestión de la Receptoría de Fondos

<div align="center">

![React](https://img.shields.io/badge/React-19.1.1-61DAFB?style=flat-square&logo=react&logoColor=black)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.8.3-3178C6?style=flat-square&logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-3.5.0-13AA52?style=flat-square&logo=mongodb&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker&logoColor=white)
![HL7-FHIR](https://img.shields.io/badge/HL7%20FHIR-R4/R5-FF6600?style=flat-square&logo=healthcare&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

**Plataforma integral de gestión de ingresos y facturación para instituciones de salud**

[Características](#-características-principales) • [Instalación](#-instalación-rápida) • [Tecnologías](#-tecnologías-utilizadas) • [Documentación](#-documentación-completa) • [Contribuir](#-contribución)

</div>

---

## 📋 Descripción General

**SIGREF** es una solución empresarial moderna diseñada para optimizar la gestión de ingresos, facturación y operaciones administrativas en instituciones de salud. El sistema implementa estándares internacionales de interoperabilidad en salud **(HL7 FHIR R4 y parcialmente R5)**, garantizando compatibilidad con sistemas de información sanitaria global.

### 🎯 ¿Qué hace SIGREF?

SIGREF automatiza y centraliza:

- ✅ **Gestión de Pacientes** - Registro conforme a estándares FHIR
- ✅ **Facturación y Control de Ingresos** - Facturas automáticas y seguimiento de pagos
- ✅ **Control de Caja** - Sesiones de pago y cuadraturas diarias
- ✅ **Reportes Financieros** - Dashboards en tiempo real con KPIs
- ✅ **Auditoría Completa** - Trazabilidad de todas las operaciones
- ✅ **Gestión Hospitalaria** - Servicios, profesionales, ubicaciones y organizaciones
- ✅ **Control de Acceso** - Autenticación segura con Keycloak (OIDC)

---

## ⭐ Características Principales

### 🏥 Módulo de Pacientes
- Registro FHIR-compliant de pacientes
- Búsqueda avanzada por identificación nacional
- Integración con servidores FHIR externos
- Historial de paciente centralizado

### 💳 Módulo de Facturación
- Generación automática de facturas con series correlativas
- Múltiples métodos de pago (efectivo)
- Reportes de ingresos por período y método de pago
- Documentos imprimibles: Facturas y recibos de caja. 
> [!WARNING]
> **Aviso Legal:** El módulo de facturación actual es de uso administrativo interno. No cumple con los estándares del régimen de facturación de la SAR y no es válido para deducción de impuestos.

### 💰 Módulo de Caja
- Apertura y cierre de sesiones de caja
- Registro de movimientos financieros
- Cuadratura automática de caja
- Validación de cambios y diferencias

### 📊 Módulo de Reportes
- Dashboard ejecutivo con KPIs en tiempo real
- Ingresos por servicio, período y profesional
- Análisis de facturación y cobranza
- Exportación a Excel y PDF
- Gráficos interactivos y comparativos

### 🔐 Seguridad y Control
- Autenticación OIDC con Keycloak
- Control de acceso basado en roles (RBAC)
- Auditoría inmutable de operaciones
- Gestión de usuarios y permisos
- Encriptación en tránsito y validación de entrada

### 🏢 Administración
- Catálogo de servicios médicos
- Gestión de profesionales sanitarios (Practitioners)
- Configuración de ubicaciones y departamentos
- Información de organizaciones y sucursales
- Propiedades hospitalarias configurables

---

## 🚀 Tecnologías Utilizadas

### 🎨 Frontend
**React 19.1.1** • TypeScript 5.8.3 • Vite 7.1.2 • Tailwind CSS 4.1.13 • Ant Design 5.27.4

**Herramientas:** TanStack Query • Zustand • Formik + Yup • Orval • Keycloak.js • ReCharts

### ⚙️ Backend
**.NET 9.0** • ASP.NET Core • Entity Framework Core 9.0.9 • HAPI FHIR R4 5.12.2

**Características:** Aspire • Keycloak Auth • Swagger/OpenAPI • PostgreSQL 17 • MongoDB 5.0+

### 🏗️ Infraestructura
**Docker Compose** • Nginx (Reverse Proxy) • Keycloak (IAM) • Seq (Log Aggregation) • OpenTelemetry

### 🔐 Autenticación
**OIDC** • **JWT Bearer Tokens** • **Keycloak** • **Role-Based Access Control (RBAC)**

---

## 💾 Instalación Rápida

### 📋 Prerrequisitos

- Docker Desktop (con Docker Compose)
- Git
- Node.js 20+ (para desarrollo frontend)
- .NET 9.0 SDK (para desarrollo backend)

### 🐳 Despliegue con Docker (Recomendado)

```bash
# 1. Clonar repositorio
git clone https://github.com/SIGREF-UNAH/SIGREF.git
cd SIGREF

# 2. Copiar y configurar variables de entorno
cp .env.production .env

# 3. IMPORTANTE: Editar .env con tus valores
# Ver: docs/VARIABLES-DE-ENTORNO.md para detalles
nano .env  # o tu editor favorito

# 4. Levantar servicios
docker-compose up -d

# 5. Acceder a la aplicación
# Frontend: http://localhost
# API Docs: http://localhost:5000/swagger
# Logs: http://localhost:5341
# Monitoreo: http://localhost:18888
```

**Servicios que se inician automáticamente:**
- ✅ Frontend (React/Nginx) - Puerto 80
- ✅ Backend API (.NET) - Puerto 5000
- ✅ PostgreSQL - Puerto 5432
- ✅ MongoDB - Puerto 27017
- ✅ Keycloak - Puerto 8080
- ✅ HAPI FHIR - Puerto 8081
- ✅ Seq (Logs) - Puerto 5341
- ✅ Aspire Dashboard - Puerto 18888

### 💻 Desarrollo Local

**Backend:**
```bash
cd BE/SIGREF.API.AppHost
dotnet run
```

**Frontend (otra terminal):**
```bash
cd FE
npm install
npm run dev  # Acceso en http://localhost:5173
```

---

## 📂 Estructura del Proyecto

```
SIGREF/
├── BE/                              # Backend (.NET 9)
│   ├── SIGREF.API/                 # API Principal
│   │   ├── Controllers/            # REST Endpoints
│   │   ├── Services/               # Lógica de Negocio
│   │   ├── Database/               # Entity Framework & Migrations
│   │   ├── Dtos/                   # Data Transfer Objects
│   │   ├── Audit/                  # Sistema de Auditoría
│   │   └── ...
│   ├── SIGREF.API.AppHost/        # .NET Aspire Host
│   └── SIGREF.API.ServiceDefaults/ # Extensiones compartidas
│
├── FE/                              # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── features/              # Páginas por módulo
│   │   ├── shared/                # Componentes reutilizables
│   │   ├── routers/               # Rutas de la app
│   │   ├── config/                # Configuración
│   │   └── api/                   # Cliente generado por Orval
│   └── ...
│
├── docs/                            # 📚 DOCUMENTACIÓN TÉCNICA
│   ├── README.md                   # Índice de documentación
│   ├── VARIABLES-DE-ENTORNO.md     # Configuración detallada
│   ├── ARQUITECTURA.md             # Diseño técnico
│   └── BASES-DE-DATOS.md           # Schema de BD
│
├── config/                          # Configuraciones globales
├── nginx/                           # Configuración Nginx
├── docker-compose.yml              # Orquestación de servicios
└── README.md                       # Este archivo
```

---

## 📖 Documentación Completa

**Documentación técnica detallada disponible en [`/docs`](./docs/):**

- 🔧 **[Configuración de Variables](./docs/VARIABLES-DE-ENTORNO.md)** - Guía completa de `.env`, ejemplos por ambiente
- 🏗️ **[Arquitectura del Sistema](./docs/ARQUITECTURA.md)** - Capas, componentes, flujos de datos
- 🗄️ **[Bases de Datos](./docs/BASES-DE-DATOS.md)** - Schema PostgreSQL y MongoDB completo

**Documentación adicional en raíz:**
- **[MANUAL-DE-USUARIO.md](./MANUAL-DE-USUARIO.md)** - Guía para usuarios finales
- **[MANUAL-TECNICO.md](./MANUAL-TECNICO.md)** - Documentación de desarrollo
- **[README-DEPLOYMENT.md](./README-DEPLOYMENT.md)** - Guía de despliegue en producción

---

## 🔒 Seguridad

### ✅ Medidas Implementadas

**Autenticación**
- OIDC con Keycloak (estándar OpenID Connect)
- JWT Bearer tokens con validación de firma
- Renovación automática de tokens

**Autorización**
- RBAC (Role-Based Access Control)
- Validación de permisos por endpoint
- Aislamiento de datos por usuario

**Datos Sensibles**
- Encriptación en tránsito (HTTPS/TLS)
- Hashing seguro de contraseñas (bcrypt)
- Validación de entrada en todos los formularios
- Protección contra inyección SQL (EF Core)

**Auditoría**
- Registro inmutable de operaciones en MongoDB
- Trazabilidad de cambios (quién, qué, cuándo)
- Almacenamiento centralizado de logs
- TTL automático para datos antiguos

**Infraestructura**
- Containerización aislada (Docker)
- Redes privadas entre servicios
- Secrets management con variables de entorno
- Sin credenciales en código fuente

---

## 📊 Funcionalidades por Rol

| Rol | Funciones |
|-----|-----------|
| **Cajero** | Registrar pacientes, crear facturas, procesar pagos, cuadrar caja, imprimir recibos |
| **Administrador** | Todas las de Cajero + gestionar usuarios, servicios, series, auditoría, reportes avanzados |
| **Auditor** | Consultar auditoría, generar reportes de trazabilidad, analizar logs |
| **Técnico TI** | Acceso a dashboards de monitoreo, logs centralizados, telemetría |

---

## 🐛 Troubleshooting

### "Connection refused" en PostgreSQL
```bash
# Verificar que PostgreSQL esté levantado
docker-compose ps postgres

# Ver logs de PostgreSQL
docker-compose logs postgres
```

### Frontend no carga
```bash
# Limpiar cache y reinstalar
cd FE && npm cache clean --force && npm install && npm run dev
```

### Keycloak no autentica
```bash
# Acceder a panel de Keycloak
# URL: http://localhost:8080
# Usuario: sigref
# Contraseña: sigref (cambiar en PRODUCCIÓN)
```

### Logs no aparecen en Seq
```bash
# Verificar Seq está corriendo: http://localhost:5341
# Verificar API key en .env sea correcta
docker-compose logs seq
```

---

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:

1. **Fork** el repositorio
2. Crea una **rama** para tu feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** tus cambios (`git commit -m 'feat: Add AmazingFeature'`)
4. **Push** a la rama (`git push origin feature/AmazingFeature`)
5. Abre un **Pull Request**

### Estándares de Código

- **Backend**: Convenciones .NET (PascalCase clases, camelCase variables)
- **Frontend**: ESLint + Prettier (formato automático)
- **Commits**: Mensajes descriptivos en inglés
- **Tests**: Incluir tests para nuevas funciones
- **Docs**: Actualizar README y comentarios

---

## 📄 Licencia

Este proyecto está licenciado bajo la licencia **MIT** - Ver [LICENSE.MD](./LICENSE.MD)

---

## 👥 Equipo de Desarrollo

**SIGREF** es un proyecto desarrollado por el equipo de **Sistemas de Información en Salud** de la **Universidad Nacional Autónoma de Honduras (UNAH)**.

**Desarrolladores:**
- Carlos Ovidio Dubón Pineda - Full Stack
- Michael Andrey Galdamez Martinez - Frontend
- Ever Josue Garcia Leonor - Frontend
- Danilo Isaac Vides Chicas - Frontend
- Anthony Edward Miranda Fuentes - Full Stack
- Hector Rene Martinez Vega - Full Stack - Coordinador de Proyecto
- Erick Marley Arita - Full Stack
- Josue David Diaz Rodriguez - Full Stack

---

## 📞 Soporte y Contacto

Para reportar bugs, sugerencias o preguntas técnicas:

- 📧 **Email:** sigref@unah.hn
- 🐛 **Issues:** [GitHub Issues](https://github.com/SIGREF-UNAH/SIGREF/issues)
- 💬 **Discussions:** [GitHub Discussions](https://github.com/SIGREF-UNAH/SIGREF/discussions)
- 📚 **Documentación:** Ver [`/docs`](./docs/)

---

## 🎯 Roadmap

### ✅ v1.0 (Actual)
- Gestión de pacientes FHIR-compliant
- Facturación y control de cobros
- Sesiones de caja y cuadratura
- Reportes administrativos
- Auditoría inmutable
- Autenticación OIDC

---

## ✨ Features Destacados

🎯 **Interoperabilidad Global** - Cumple FHIR, compatible con sistemas internacionales

💡 **Fácil de Usar** - Interfaz intuitiva, responsive, accesible (WCAG 2.1)

⚡ **Alto Rendimiento** - Queries optimizadas, caching, lazy loading, CDN ready

🔐 **Seguridad Enterprise** - ISO 27001-like, auditoría inmutable, encriptación

🌍 **Escalable** - Cloud-native, Kubernetes ready, bases de datos replicables

---

## 🙏 Agradecimientos

- **HL7 International** - Estándares FHIR
- **Microsoft** - .NET y ASP.NET Core
- **Facebook** - React
- **Comunidad Open Source** - Librerías utilizadas

---

**Última actualización:** 31 de enero de 2026

Hecho con ❤️ por el equipo de SIGREF
