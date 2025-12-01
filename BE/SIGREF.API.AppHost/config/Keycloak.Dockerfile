FROM quay.io/keycloak/keycloak:latest

# Copiar temas personalizados
COPY ./themes /opt/keycloak/themes

# Usar el entrypoint por defecto
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
