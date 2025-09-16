#!/bin/bash

# Script de inicialización de Keycloak para FHIR
# Este script configura automáticamente Keycloak con el realm FHIR

set -e

KEYCLOAK_URL="http://keycloak:8080"
ADMIN_USER="admin"
ADMIN_PASSWORD="admin"
REALM_NAME="fhir"

echo "🚀 Iniciando configuración de Keycloak..."

# Función para esperar que Keycloak esté listo
wait_for_keycloak() {
    echo "⏳ Esperando a que Keycloak esté disponible..."
    while ! curl -f -s "${KEYCLOAK_URL}/health/ready" > /dev/null 2>&1; do
        sleep 5
        echo "   ... esperando"
    done
    echo "✅ Keycloak está listo!"
}

# Función para obtener token de acceso
get_access_token() {
    echo "🔐 Obteniendo token de acceso..."
    TOKEN=$(curl -s -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "username=${ADMIN_USER}" \
        -d "password=${ADMIN_PASSWORD}" \
        -d "grant_type=password" \
        -d "client_id=admin-cli" | jq -r '.access_token')
    
    if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
        echo "❌ Error: No se pudo obtener el token de acceso"
        exit 1
    fi
    echo "✅ Token obtenido exitosamente"
}

# Función para verificar si el realm existe
realm_exists() {
    local response=$(curl -s -o /dev/null -w "%{http_code}" \
        -H "Authorization: Bearer ${TOKEN}" \
        "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}")
    
    if [ "$response" = "200" ]; then
        return 0
    else
        return 1
    fi
}

# Función para crear el realm
create_realm() {
    echo "🏗️  Creando realm '${REALM_NAME}'..."
    
    if realm_exists; then
        echo "⚠️  El realm '${REALM_NAME}' ya existe. Eliminándolo..."
        curl -s -X DELETE \
            -H "Authorization: Bearer ${TOKEN}" \
            "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}"
    fi
    
    # Importar la configuración del realm desde el archivo JSON
    curl -s -X POST "${KEYCLOAK_URL}/admin/realms" \
        -H "Authorization: Bearer ${TOKEN}" \
        -H "Content-Type: application/json" \
        -d @/opt/keycloak/data/import/keycloak-realm.json
    
    if [ $? -eq 0 ]; then
        echo "✅ Realm '${REALM_NAME}' creado exitosamente"
    else
        echo "❌ Error al crear el realm"
        exit 1
    fi
}

# Función para verificar la configuración
verify_setup() {
    echo "🔍 Verificando configuración..."
    
    # Verificar que el realm existe
    if realm_exists; then
        echo "✅ Realm verificado"
    else
        echo "❌ Error: El realm no fue creado correctamente"
        exit 1
    fi
    
    # Verificar clientes
    local clients=$(curl -s -H "Authorization: Bearer ${TOKEN}" \
        "${KEYCLOAK_URL}/admin/realms/${REALM_NAME}/clients" | jq -r '.[].clientId')
    
    if echo "$clients" | grep -q "fhir-client"; then
        echo "✅ Cliente 'fhir-client' configurado"
    else
        echo "❌ Error: Cliente 'fhir-client' no encontrado"
    fi
    
    if echo "$clients" | grep -q "fhir-admin"; then
        echo "✅ Cliente 'fhir-admin' configurado"
    else
        echo "❌ Error: Cliente 'fhir-admin' no encontrado"
    fi
}

# Función para mostrar información de configuración
show_info() {
    echo ""
    echo "🎉 ¡Configuración de Keycloak completada!"
    echo ""
    echo "📋 Información de acceso:"
    echo "   • URL de Keycloak: http://localhost:8081"
    echo "   • Consola de administración: http://localhost:8081/admin"
    echo "   • Usuario administrador: ${ADMIN_USER}"
    echo "   • Contraseña administrador: ${ADMIN_PASSWORD}"
    echo ""
    echo "🔑 Realm FHIR creado con:"
    echo "   • Realm: ${REALM_NAME}"
    echo "   • URL del realm: http://localhost:8081/realms/${REALM_NAME}"
    echo ""
    echo "👥 Usuarios de prueba creados:"
    echo "   • admin / admin123 (Administrador)"
    echo "   • practitioner / practitioner123 (Profesional de salud)"
    echo "   • patient / patient123 (Paciente)"
    echo ""
    echo "🔧 Clientes configurados:"
    echo "   • fhir-client (secret: fhir-client-secret)"
    echo "   • fhir-admin (secret: fhir-admin-secret)"
    echo ""
    echo "🌐 URLs útiles:"
    echo "   • Token endpoint: http://localhost:8081/realms/${REALM_NAME}/protocol/openid-connect/token"
    echo "   • User info endpoint: http://localhost:8081/realms/${REALM_NAME}/protocol/openid-connect/userinfo"
    echo "   • Logout endpoint: http://localhost:8081/realms/${REALM_NAME}/protocol/openid-connect/logout"
    echo ""
}

# Ejecutar configuración
main() {
    wait_for_keycloak
    get_access_token
    create_realm
    verify_setup
    show_info
}

# Ejecutar si se llama directamente
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi