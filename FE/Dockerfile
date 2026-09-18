# syntax=docker/dockerfile:1.7

# Build stage: dependencies are cached independently from application sources.
FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm \
    npm ci --legacy-peer-deps

# Only files required by Vite and TypeScript are copied into the build stage.
COPY index.html vite.config.ts tsconfig.json tsconfig.app.json tsconfig.node.json tsconfig.test.json ./
COPY src ./src
COPY tests ./tests

ARG VITE_KEYCLOAK_URL
ARG VITE_KEYCLOAK_REALM
ARG VITE_KEYCLOAK_CLIENT_ID
ARG VITE_API_URL

ENV VITE_KEYCLOAK_URL=${VITE_KEYCLOAK_URL} \
    VITE_KEYCLOAK_REALM=${VITE_KEYCLOAK_REALM} \
    VITE_KEYCLOAK_CLIENT_ID=${VITE_KEYCLOAK_CLIENT_ID} \
    VITE_API_URL=${VITE_API_URL}

RUN npm run build


# Runtime stage: contains only Nginx and the generated static site.
FROM nginx:stable-alpine AS runtime

LABEL org.opencontainers.image.authors="@TETvega" \
      org.opencontainers.image.title="SIGREF Frontend" \
      org.opencontainers.image.description="Production-ready static SPA server"

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build --chown=nginx:nginx /app/dist /usr/share/nginx/html

# Nginx runs as an unprivileged user and owns only its writable directories.
RUN touch /var/run/nginx.pid \
    && chown nginx:nginx /var/run/nginx.pid \
    && chown -R nginx:nginx /var/cache/nginx

USER nginx

# EXPOSE documents the single application port; publishing is controlled by the runtime.
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1:8080/healthz || exit 1

CMD ["nginx", "-g", "daemon off;"]
