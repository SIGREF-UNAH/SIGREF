# Sistema de Gestión de la Receptoría de Fondos (SIGREF)

![React](https://img.shields.io/badge/React-19.1.1-61DAFB?style=flat-square&logo=react&logoColor=black)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.8.3-3178C6?style=flat-square&logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?style=flat-square&logo=docker&logoColor=white)

## 📋 Descripción

SIGREF es una plataforma web moderna destinada a modernizar la gestión y control de ingresos en servicios médicos. El sistema permite:

- ✅ Registrar ingresos y facturación de servicios médicos
- ✅ Gestionar información de pacientes (FHIR-compliant)
- ✅ Administrar catálogo de servicios y paquetes
- ✅ Controlar sesiones de caja y turnos
- ✅ Generar reportes administrativos y financieros
- ✅ Auditar todas las operaciones del sistema
- ✅ Gestionar empleados, ubicaciones y organizaciones

> [!NOTE]  
> Este proyecto sigue los estándares **HL7 FHIR R5**, siendo un **sistema FHIR-LIKE** que adopta las mejores prácticas de interoperabilidad en salud.

---

## 🚀 Tecnologías Utilizadas

### Frontend
- **React** 19.1.1 - Framework UI
- **TypeScript** 5.8.3 - Lenguaje tipado
- **Vite** 7.1.2 - Build tool
- **Tailwind CSS** 4.1.13 - Framework CSS
- **Ant Design** 5.27.4 - Componentes UI
- **TanStack Query** 5.90.2 - Data fetching
- **Zustand** 5.0.8 - State management
- **Formik** + **Yup** - Formularios y validación
- **Orval** 7.13.1 - Generador de cliente API

### Backend
- **.NET** 9.0 - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** 9.0.9 - ORM
- **HAPI FHIR** 5.12.2 - Cliente FHIR R4
- **PostgreSQL** 17 - Base de datos relacional
- **MongoDB** - Base de datos de logs
- **Keycloak** - Identity & Access Management

### Infraestructura
- **Docker** + **Docker Compose** - Containerización
- **Nginx** - Reverse proxy
- **Cloudflare Tunnel** - Acceso seguro
- **OpenTelemetry** - Observabilidad
- **Aspire Dashboard** - Monitoreo
- **Seq** - Agregación de logs

---

## 📁 Estructura del Repositorio

```
SIGREF/
├── BE/                          # Backend (.NET 9)
│   ├── SIGREF.API/             # API principal
│   └── SIGREF.API.ServiceDefaults/  # Configuración Aspire
├── FE/                          # Frontend (React + TypeScript)
│   ├── src/                    # Código fuente
│   ├── public/                 # Assets estáticos
│   └── documentation/          # Documentación
├── config/                      # Configuraciones
│   ├── init-databases.sql      # Script de inicialización DB
│   └── otel-collector-config.yaml  # Config OpenTelemetry
├── nginx/                       # Configuración Nginx
├── docker-compose.yml          # Orquestación de servicios
├── .env.production             # Variables de entorno
├── MANUAL-DE-USUARIO.md        # Manual de usuario
├── MANUAL-TECNICO.md           # Manual técnico
├── README-DEPLOYMENT.md        # Guía de despliegue
└── README.md                   # Este archivo
```

---

## 🚀 Inicio Rápido

### Prerrequisitos

- Docker Desktop instalado
- Git
- Node.js 20+ (para desarrollo frontend)
- .NET 9.0 SDK (para desarrollo backend)

### Instalación con Docker

```bash
# Clonar el repositorio
git clone https://github.com/SIGREF-UNAH/SIGREF.git
cd SIGREF

# Configurar variables de entorno
cp .env.production .env
# Editar .env con tus valores

# Iniciar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f
```

### Desarrollo Local

**Backend:**
```bash
cd BE/SIGREF.API
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend:**
```bash
cd FE
npm install
npm run orval  # Generar cliente API
npm run dev
```

---

## 📚 Documentación

- **[Manual de Usuario](./MANUAL-DE-USUARIO.md)** - Guía completa para usuarios finales
- **[Manual Técnico](./MANUAL-TECNICO.md)** - Documentación técnica y de código
- **[Guía de Despliegue](./README-DEPLOYMENT.md)** - Instrucciones de despliegue en producción
- **[Documentación Frontend](./FE/documentation/index.md)** - Componentes y hooks reutilizables

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                      CLOUDFLARE CDN                         │
│                  (SSL, DDoS Protection)                     │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                   NGINX (Reverse Proxy)                     │
│  ┌──────────┬──────────┬──────────┬──────────────────────┐ │
│  │ Frontend │ API      │ Keycloak │ Aspire Dashboard     │ │
│  │ (React)  │ (.NET 9) │ (Auth)   │ (Monitoring)         │ │
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

---

## 🔐 Roles del Sistema

| Rol | Descripción | Permisos |
|-----|-------------|----------|
| **Administrador** | Control total del sistema | Gestión completa de todos los módulos |
| **Auxiliar de Caja** | Registro de ingresos | Crear ingresos, cerrar caja, gestionar pacientes |
| **Auditor** | Supervisión y auditoría | Consulta de ingresos, reportes, eventos |
| **Técnico de Informática** | Configuración técnica | Gestión de usuarios, ubicaciones, organizaciones |

---

## 🧑‍💻 Equipo de Desarrollo

| Nombre | Rol | GitHub | Contribuciones |
|--------|-----|--------|----------------|
| Carlos Ovidio Dubón Pineda | Backend Developer | [@Pineda04](https://github.com/Pineda04) | [Ver](./realizados/carlos.md) |
| Michael Andrey Galdamez Martinez | Frontend Developer | [@MichaelGald](https://github.com/MichaelGald) | [Ver](./realizados/michael.md) |
| Ever Josue Garcia Leonor | Backend Developer | [@everjosue56](https://github.com/everjosue56) | [Ver](./realizados/ever.md) |
| Danilo Isaac Vides Chicas | Frontend Developer | [@IsaacV04](https://github.com/IsaacV04) | [Ver](./realizados/danilo.md) |
| Anthony Edward Miranda Fuentes | Backend Developer | [@AnthonyEMF](https://github.com/AnthonyEMF) | [Ver](./realizados/anthony.md) |
| Hector Rene Martinez Vega | Backend Developer | [@TETvega](https://github.com/TETvega) | [Ver](./realizados/hector.md) |
| Erick Marley Arita | Full Stack Developer | [@erickArita](https://github.com/erickArita) | [Ver](./realizados/erick.md) |
| Josue David Diaz Rodriguez | Full Stack Developer | [@JDDR200530](https://github.com/JDDR200530) | [Ver](./realizados/josue.md) |

---

## 📝 Licencia

Este proyecto está bajo la licencia especificada en [LICENSE.MD](./LICENSE.MD)

---

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'feat: Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📞 Soporte

Para soporte técnico o preguntas:

- **Issues**: [GitHub Issues](https://github.com/SIGREF-UNAH/SIGREF/issues)
- **Documentación**: Ver manuales en el repositorio
- **Email**: Contactar al equipo de desarrollo

---

**© 2024 SIGREF - Sistema de Gestión de la Receptoría de Fondos**

*Desarrollado por estudiantes de Ingeniería en Sistemas - UNAH*


