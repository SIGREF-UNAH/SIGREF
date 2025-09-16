#!/bin/sh
set -e

echo "Esperando a que Keycloak esté disponible..."
until curl -f http://keycloak-server:8080/health/ready; do
    echo "Keycloak no está listo, esperando..."
    sleep 5
done

echo "Keycloak está listo, configurando realm..."

# Obtener token de acceso
ACCESS_TOKEN=$(curl -s -X POST http://keycloak-server:8080/realms/master/protocol/openid-connect/token \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=admin" \
    -d "password=admin" \
    -d "grant_type=password" \
    -d "client_id=admin-cli" | \
    sed -n 's/.*"access_token":"\([^"]*\)".*/\1/p')

if [ -z "$ACCESS_TOKEN" ]; then
    echo "Error: No se pudo obtener el token de acceso"
    exit 1
fi

echo "Token obtenido exitosamente"

# Verificar si el realm ya existe
REALM_EXISTS=$(curl -s -o /dev/null -w "%{http_code}" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    http://keycloak-server:8080/admin/realms/fhir)

if [ "$REALM_EXISTS" = "200" ]; then
    echo "El realm 'fhir' ya existe, no es necesario crearlo"
    exit 0
fi

# Crear el realm
echo "Creando realm 'fhir'..."
RESPONSE=$(curl -s -w "%{http_code}" -o /tmp/response.json \
    -X POST http://keycloak-server:8080/admin/realms \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d @/scripts/keycloak-realm.json)

if [ "$RESPONSE" = "201" ]; then
    echo "Realm 'fhir' creado exitosamente"
else
    echo "Error al crear el realm. Código de respuesta: $RESPONSE"
    cat /tmp/response.json
    exit 1
fi

echo "Configuración de Keycloak completada"