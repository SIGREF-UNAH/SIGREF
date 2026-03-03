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


# ----------------------------------------------------------------------------
# 1. ETAPA DE CONSTRUCCIÓN (Builder)
# ----------------------------------------------------------------------------
# En esta etapa se pre-configura el servidor para evitar que lo haga en cada inicio.
FROM quay.io/keycloak/keycloak:latest AS builder

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
FROM quay.io/keycloak/keycloak:latest

# Copiamos solo el resultado de la optimización desde la etapa anterior.
COPY --from=builder /opt/keycloak/ /opt/keycloak/

# Preparación para la importación del Realm (SIGREF).
# Se coloca en el directorio de importación oficial de Keycloak.
COPY ./sigref-realm-full.json /opt/keycloak/data/import/sigref-realm.json

# VARIABLES DE ENTORNO DE TIEMPO DE EJECUCIÓN
# KC_IMPORT: Indica al servidor que busque archivos de configuración al arrancar.
ENV KC_IMPORT=/opt/keycloak/data/import/sigref-realm.json
# Recomendado: Desactivar el modo developer para producción
# ENV KC_HTTP_RELATIVE_PATH=/auth

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]

# COMANDO DE INICIO OPTIMIZADO:
# 'start' con '--optimized' le dice a Keycloak que ignore las comprobaciones 
# de configuración porque ya se hicieron en la etapa de 'build'.
# '--import-realm' permite cargar el JSON si la base de datos está vacía.
CMD ["start", "--optimized", "--import-realm"]

# ----------------------------------------------------------------------------
# REFERENCIAS TÉCNICAS:
# - Keycloak Guides (Optimizing the image): https://www.keycloak.org/server/containers
# - Keycloak Database Configuration: https://www.keycloak.org/server/db
# - Keycloak Quarkus Migration: https://www.keycloak.org/migration/migrating-to-quarkus
# ----------------------------------------------------------------------------