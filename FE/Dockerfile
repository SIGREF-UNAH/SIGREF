# ============================================================================
# MÓDULO: Frontend Web Application (Vite + Node.js)
# PROYECTO: SIGREF (Sistema de Gestión y Referencia)
# ----------------------------------------------------------------------------
# AUTOR (GitHub): 
#   - @TETvega (Héctor Rene Martínez Vega) -> Arquitectura y Orquestación
# CO-AUTOR DE OPTIMIZACIÓN: 
#   - Gemini 3 Flash (Google AI) -> Estrategia de Multi-staging y Seguridad Alpine
# ----------------------------------------------------------------------------
# PROPÓSITO: Build de producción optimizado para Single Page Application (SPA),
#            con inyección de variables de entorno y servidor Nginx endurecido.
# ============================================================================

# --- STAGE 1: COMPILACIÓN (BUILDER) ---
# Usamos Node 20 sobre Alpine para reducir la superficie de ataque
FROM node:20-alpine AS build

WORKDIR /app

# Optimizamos el caché de capas: Instalación de dependencias
# Se copian solo los archivos de manifiesto para evitar reinstalaciones innecesarias
COPY package*.json ./

# Instalación 'Clean Install' (ci) para asegurar versiones exactas del lockfile
# Se limpia el caché inmediatamente para mantener la capa del builder ligera
RUN npm ci --legacy-peer-deps && npm cache clean --force

# Inyección de Argumentos de Construcción (Build Args)
# Estas variables se graban en el bundle de JS durante el proceso de build
ARG VITE_KEYCLOAK_URL
ARG VITE_KEYCLOAK_REALM
ARG VITE_KEYCLOAK_CLIENT_ID
ARG VITE_API_URL

ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL \
    VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM \
    VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID \
    VITE_API_URL=$VITE_API_URL

# Copia del código fuente y generación de artefactos de producción
COPY . .
RUN npm run build:prod


# --- STAGE 2: SERVIDOR DE PRODUCCIÓN (RUNTIME) ---
# Imagen ultra-ligera de Nginx para servir contenido estático
FROM nginx:stable-alpine

# Metadatos OCI para trazabilidad de la imagen
LABEL org.opencontainers.image.authors="@TETvega"
LABEL org.opencontainers.image.title="SIGREF Frontend"
LABEL org.opencontainers.image.description="Servidor Nginx optimizado para SPA"

# Reemplazo de configuración de Nginx (Manejo de rutas internas de la SPA)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Transferencia de artefactos compilados desde el builder
COPY --from=build /app/dist /usr/share/nginx/html

# ESTRATEGIA DE SEGURIDAD:
# 1. Creamos el archivo PID y ajustamos permisos para carpetas de Nginx
# 2. Permitimos que el proceso corra sin privilegios de root
RUN touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid /var/cache/nginx /usr/share/nginx/html

# Exposición del puerto de servicio
EXPOSE 80

# Cambio a usuario no privilegiado (Seguridad recomendada por Docker & Kubernetes)
USER nginx

# Monitoreo de salud del contenedor
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:80/ || exit 1

# Ejecución de Nginx en primer plano
CMD ["nginx", "-g", "daemon off;"]