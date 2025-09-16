#!/bin/bash

# Script de utilidades para Keycloak FHIR
# Proporciona comandos comunes para gestionar la configuración

KEYCLOAK_URL="http://localhost:8081"
REALM="fhir"

show_help() {
    echo "🔐 Utilidades de Keycloak FHIR"
    echo ""
    echo "Uso: $0 [COMANDO] [OPCIONES]"
    echo ""
    echo "COMANDOS:"
    echo "  status              - Verificar estado de Keycloak"
    echo "  token [usuario]     - Obtener token para usuario"
    echo "  users               - Listar usuarios del realm"
    echo "  clients             - Listar clientes del realm"
    echo "  reset               - Reinicializar configuración"
    echo "  test-auth [usuario] - Probar autenticación"
    echo "  help                - Mostrar esta ayuda"
    echo ""
    echo "EJEMPLOS:"
    echo "  $0 status"
    echo "  $0 token practitioner"
    echo "  $0 test-auth admin"
    echo ""
}

check_dependencies() {
    if ! command -v curl &> /dev/null; then
        echo "❌ Error: curl no está instalado"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        echo "❌ Error: jq no está instalado"
        echo "   Instalar con: sudo apt-get install jq"
        exit 1
    fi
}

get_admin_token() {
    curl -s -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "username=admin" \
        -d "password=admin" \
        -d "grant_type=password" \
        -d "client_id=admin-cli" | jq -r '.access_token'
}

check_status() {
    echo "🔍 Verificando estado de Keycloak..."
    
    # Verificar si Keycloak responde
    if curl -f -s "${KEYCLOAK_URL}/health" > /dev/null; then
        echo "✅ Keycloak está ejecutándose"
    else
        echo "❌ Keycloak no está disponible"
        return 1
    fi
    
    # Verificar realm FHIR
    if curl -f -s "${KEYCLOAK_URL}/realms/${REALM}" > /dev/null; then
        echo "✅ Realm FHIR configurado"
    else
        echo "❌ Realm FHIR no encontrado"
        return 1
    fi
    
    echo "✅ Todo funciona correctamente"
}

get_user_token() {
    local username=$1
    local password
    
    if [ -z "$username" ]; then
        echo "❌ Error: Debe especificar un usuario"
        echo "   Usuarios disponibles: admin, practitioner, patient"
        return 1
    fi
    
    # Contraseñas por defecto
    case $username in
        "admin") password="admin123" ;;
        "practitioner") password="practitioner123" ;;
        "patient") password="patient123" ;;
        *) 
            echo "❌ Usuario no reconocido: $username"
            echo "   Usuarios disponibles: admin, practitioner, patient"
            return 1
            ;;
    esac
    
    echo "🔑 Obteniendo token para usuario: $username"
    
    local response=$(curl -s -X POST "${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "grant_type=password" \
        -d "client_id=fhir-client" \
        -d "client_secret=fhir-client-secret" \
        -d "username=$username" \
        -d "password=$password")
    
    local token=$(echo "$response" | jq -r '.access_token // empty')
    
    if [ -n "$token" ] && [ "$token" != "null" ]; then
        echo "✅ Token obtenido exitosamente:"
        echo "$token"
        echo ""
        echo "📋 Información del token:"
        echo "$response" | jq '{
            access_token: .access_token[0:50] + "...",
            token_type: .token_type,
            expires_in: .expires_in,
            refresh_expires_in: .refresh_expires_in,
            scope: .scope
        }'
    else
        echo "❌ Error al obtener token:"
        echo "$response" | jq '.'
        return 1
    fi
}

list_users() {
    echo "👥 Listando usuarios del realm FHIR..."
    
    local admin_token=$(get_admin_token)
    
    if [ "$admin_token" = "null" ] || [ -z "$admin_token" ]; then
        echo "❌ Error: No se pudo obtener token de administrador"
        return 1
    fi
    
    local users=$(curl -s -H "Authorization: Bearer $admin_token" \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/users")
    
    echo "$users" | jq -r '.[] | "• \(.username) (\(.firstName) \(.lastName)) - \(.email) - Activo: \(.enabled)"'
}

list_clients() {
    echo "🔧 Listando clientes del realm FHIR..."
    
    local admin_token=$(get_admin_token)
    
    if [ "$admin_token" = "null" ] || [ -z "$admin_token" ]; then
        echo "❌ Error: No se pudo obtener token de administrador"
        return 1
    fi
    
    local clients=$(curl -s -H "Authorization: Bearer $admin_token" \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/clients")
    
    echo "$clients" | jq -r '.[] | select(.clientId | startswith("fhir-")) | "• \(.clientId) - \(.name // "Sin nombre") - Activo: \(.enabled)"'
}

test_auth() {
    local username=$1
    
    if [ -z "$username" ]; then
        echo "❌ Error: Debe especificar un usuario"
        return 1
    fi
    
    echo "🧪 Probando autenticación para: $username"
    
    # Obtener token
    local token_response=$(get_user_token "$username")
    
    if [ $? -eq 0 ]; then
        # Extraer solo el token de la respuesta
        local token=$(echo "$token_response" | grep -E '^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$' | head -1)
        
        if [ -n "$token" ]; then
            echo ""
            echo "🔍 Probando endpoint de información del usuario..."
            local userinfo=$(curl -s -H "Authorization: Bearer $token" \
                "${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/userinfo")
            
            echo "✅ Información del usuario:"
            echo "$userinfo" | jq '.'
        fi
    fi
}

reset_config() {
    echo "🔄 Reinicializando configuración de Keycloak..."
    echo ""
    echo "⚠️  ADVERTENCIA: Esto eliminará todos los datos actuales"
    echo "¿Está seguro de que desea continuar? [y/N]"
    
    read -r response
    if [[ ! "$response" =~ ^[Yy]$ ]]; then
        echo "❌ Operación cancelada"
        return 1
    fi
    
    echo "🛑 Deteniendo servicios..."
    docker compose -f HappyFHIR/docker-compose.yml down
    
    echo "🗑️  Eliminando volúmenes..."
    docker volume rm happyfhir_db_data 2>/dev/null || true
    
    echo "🚀 Reiniciando servicios..."
    docker compose -f HappyFHIR/docker-compose.yml up -d --build
    
    echo "✅ Configuración reinicializada. Espere unos minutos para que todo esté listo."
}

# Función principal
main() {
    check_dependencies
    
    case "${1:-help}" in
        "status")
            check_status
            ;;
        "token")
            get_user_token "$2"
            ;;
        "users")
            list_users
            ;;
        "clients")
            list_clients
            ;;
        "test-auth")
            test_auth "$2"
            ;;
        "reset")
            reset_config
            ;;
        "help"|*)
            show_help
            ;;
    esac
}

# Ejecutar función principal
main "$@"