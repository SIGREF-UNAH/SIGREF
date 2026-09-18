# syntax=docker/dockerfile:1.7
# ============================================================================
# CAMBIOS APLICADOS (Codex, 2026-09-17)
# - Contexto reducido mediante config/.dockerignore para no enviar archivos ajenos.
# - Ejecucion explicita como usuario no root y permisos de los artefactos ajustados.
# - Puertos HTTP y de administracion documentados; credenciales solo en runtime.
# ============================================================================
#
# ============================================================================
# MÓDULO: Infraestructura de Autenticación (Keycloak)
# PROYECTO: SIGREF
# ----------------------------------------------------------------------------
# AUTORES (GitHub):
#   - @erickArita (Erick Marley Arita) -> Estructura inicial y configuración
#   - @TETvega (Héctor Rene Martínez Vega) -> Mejora de integración y lógica
# CO-AUTOR DE OPTIMIZACIÓN Y VALIDACIÓN:
#   - Gemini 3 Flash (Google AI) -> Optimización de capas y Build-time Quarkus
# ----------------------------------------------------------------------------
# PROPÓSITO: Implementación de alto rendimiento basada en Quarkus con 
#            pre-build de base de datos y temas personalizados.
# ============================================================================

# Versión estable fijada. Cambiar solo tras validación explícita.
ARG KEYCLOAK_VERSION=26.5.5-0
# ----------------------------------------------------------------------------
# 1. ETAPA DE CONSTRUCCIÓN (Builder)
# ----------------------------------------------------------------------------
# En esta etapa se pre-configura el servidor para evitar que lo haga en cada inicio.
FROM quay.io/keycloak/keycloak:${KEYCLOAK_VERSION} AS builder

# Configuración de Build-time: Estas variables se graban en la imagen optimizada.
# Al definir KC_DB aquí, el driver de Postgres se prepara de antemano.
ENV KC_HEALTH_ENABLED=true
ENV KC_METRICS_ENABLED=true
ENV KC_DB=postgres

WORKDIR /opt/keycloak

# Copiar temas personalizados para que el comando 'build' los reconozca.
COPY ./themes /opt/keycloak/themes

# Ejecuta la optimización. Esto genera un nuevo "server image" que arranca 
# instantáneamente al no tener que buscar drivers o temas en tiempo de ejecución.
RUN /opt/keycloak/bin/kc.sh build

# ----------------------------------------------------------------------------
# 2. ETAPA FINAL (Runtime)
# ----------------------------------------------------------------------------
FROM quay.io/keycloak/keycloak:${KEYCLOAK_VERSION} AS final

# Keycloak escucha internamente en HTTP y expone administracion solo en la red Docker.
EXPOSE 8080
EXPOSE 9000

# Copiamos solo el resultado optimizado y lo asignamos al usuario no privilegiado.
COPY --chown=1000:0 --from=builder /opt/keycloak/ /opt/keycloak/

# ----------------------------------------------------------------------------
# IMPORTACIÓN DEL REALM PERSONALIZADO (SIGREF)
# ----------------------------------------------------------------------------
# El archivo sigref-realm-full.json debe existir en config/ junto a este Dockerfile.
# Keycloak con --import-realm importa este archivo SOLO si el realm "sigref"
# no existe aún en la base de datos. Es una operación idempotente y segura:
# si el realm ya existe (reinicios normales), no lo sobreescribe ni lo duplica.
# ----------------------------------------------------------------------------
COPY --chown=1000:0 ./realm-full-export.json /opt/keycloak/data/import/sigref-realm.json

# NOTA: KC_IMPORT como variable de entorno está deprecada desde Keycloak 20+.
# El mecanismo correcto es --import-realm en el CMD, que ya está abajo.
# No se define KC_IMPORT aquí intencionalmente.

# La imagen oficial reserva UID 1000 para Keycloak; no se ejecuta como root.
USER 1000
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]

# COMANDO DE INICIO OPTIMIZADO:
# --optimized  -> Omite comprobaciones de config (ya hechas en build-stage).
# --import-realm -> Carga /opt/keycloak/data/import/*.json si el realm no existe.
CMD ["start", "--optimized", "--import-realm"]

# ----------------------------------------------------------------------------
# REFERENCIAS TÉCNICAS:
# - Keycloak Guides (Optimizing the image): https://www.keycloak.org/server/containers
# - Keycloak Database Configuration: https://www.keycloak.org/server/db
# - Keycloak Import/Export: https://www.keycloak.org/server/importExport
# ----------------------------------------------------------------------------
