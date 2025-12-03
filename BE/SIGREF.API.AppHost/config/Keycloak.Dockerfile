FROM quay.io/keycloak/keycloak:latest

# Copiar temas personalizados
COPY ./themes /opt/keycloak/themes

# Copiar realm inicial para import automático
COPY ./sigref-realm-full.json /opt/keycloak/data/import/sigref-realm.json

# Usar el entrypoint por defecto
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
