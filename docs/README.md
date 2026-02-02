# 📚 Documentación de SIGREF

Bienvenido a la documentación técnica de SIGREF. Aquí encontrarás guías detalladas sobre arquitectura, configuración y desarrollo del proyecto.

---

## 📖 Guías Disponibles

### 🔧 [Configuración de Variables de Entorno](./VARIABLES-DE-ENTORNO.md)
Descripción completa de todas las variables de entorno necesarias, con ejemplos para diferentes ambientes (desarrollo, staging, producción).

**Temas cubiertos:**
- PostgreSQL, Keycloak, API .NET
- Frontend React, Logging y Monitoreo
- Integración FHIR
- Checklist de cambios para producción
- Generación segura de contraseñas

---

### 🏗️ [Arquitectura Técnica](./ARQUITECTURA.md)
Explicación detallada de la arquitectura del sistema, incluyendo capas, componentes y flujos.

**Temas cubiertos:**
- Capas (Presentación, Aplicación, Datos)
- Flujo completo de una solicitud HTTP
- Controllers, Services, Entity Framework
- Patrones de seguridad (autenticación, autorización, auditoría)
- Integración FHIR
- Despliegue en Docker

---

### 🗄️ [Bases de Datos](./BASES-DE-DATOS.md)
Esquema completo de PostgreSQL y MongoDB, con tablas, relaciones e índices.

**Temas cubiertos:**
- Schema de PostgreSQL (15 tablas principales)
- Colecciones de MongoDB (auditoría, logs, sesiones)
- Relaciones y claves foráneas
- Vistas materializadas
- Sincronización de datos
- Seguridad y backups

---

## 🚀 Guía Rápida de Inicio

### Para Usuarios Nuevos

1. Lee [README.md](../README.md) para entender qué es SIGREF
2. Lee [VARIABLES-DE-ENTORNO.md](./VARIABLES-DE-ENTORNO.md) para configurar tu entorno
3. Ejecuta `docker-compose up` en la raíz del proyecto

### Para Desarrolladores Backend

1. Lee [ARQUITECTURA.md](./ARQUITECTURA.md) - Capas y Components
2. Lee [BASES-DE-DATOS.md](./BASES-DE-DATOS.md) - Schema PostgreSQL
3. Revisa los Controllers en `BE/SIGREF.API/Controllers/`
4. Revisa los Services en `BE/SIGREF.API/Services/`

### Para Desarrolladores Frontend

1. Lee [ARQUITECTURA.md](./ARQUITECTURA.md) - Capa de Presentación
2. Revisa la estructura en `FE/src/`
3. Ejecuta `npm run orval` para generar cliente API
4. Revisa componentes compartidos en `FE/src/shared/components/`

### Para DevOps/SRE

1. Lee [VARIABLES-DE-ENTORNO.md](./VARIABLES-DE-DATOS.md) - Configuración completa
2. Lee [ARQUITECTURA.md](./ARQUITECTURA.md) - Sección Despliegue en Docker
3. Lee [BASES-DE-DATOS.md](./BASES-DE-DATOS.md) - Backup & Recovery

---

## 🎯 Preguntas Frecuentes

### ¿Dónde cambio la contraseña de PostgreSQL?
Ver [VARIABLES-DE-ENTORNO.md](./VARIABLES-DE-ENTORNO.md) - Sección PostgreSQL

### ¿Cómo agrego un nuevo rol al sistema?
Ver [ARQUITECTURA.md](./ARQUITECTURA.md) - Sección RBAC

### ¿Cuál es el schema de la tabla Invoices?
Ver [BASES-DE-DATOS.md](./BASES-DE-DATOS.md) - Tabla Invoices

### ¿Cómo se registra la auditoría?
Ver [ARQUITECTURA.md](./ARQUITECTURA.md) - Sección Auditoría

### ¿Cómo integro con un servidor FHIR externo?
Ver [ARQUITECTURA.md](./ARQUITECTURA.md) - Sección Integración FHIR

---

## 📋 Estructura de Documentación

```
docs/
├── README.md                      # Este archivo (índice)
├── VARIABLES-DE-ENTORNO.md       # Configuración completa
├── ARQUITECTURA.md                # Diseño técnico
└── BASES-DE-DATOS.md             # Schema y modelos de datos
```

---

## 🔄 Relacionado

- **[README Principal](../README.md)** - Descripción general del proyecto
- **[MANUAL-DE-USUARIO.md](../MANUAL-DE-USUARIO.md)** - Guía para usuarios finales
- **[MANUAL-TECNICO.md](../MANUAL-TECNICO.md)** - Documentación técnica detallada
- **[README-DEPLOYMENT.md](../README-DEPLOYMENT.md)** - Guía de despliegue

---

## 🤝 Contribuir a la Documentación

Si encuentras errores o tienes mejoras:

1. Fork el repositorio
2. Crea una rama: `git checkout -b docs/mi-mejora`
3. Realiza cambios
4. Commit: `git commit -m 'docs: Mejorar documentación de BD'`
5. Push: `git push origin docs/mi-mejora`
6. Abre un Pull Request

---

## 📞 Soporte

- **Issues:** [GitHub Issues](https://github.com/SIGREF-UNAH/SIGREF/issues)
- **Discussions:** [GitHub Discussions](https://github.com/SIGREF-UNAH/SIGREF/discussions)
- **Email:** sigref@unah.hn

---

**Última actualización:** 31 de enero de 2026

*Hecho con ❤️ por el equipo de SIGREF*
