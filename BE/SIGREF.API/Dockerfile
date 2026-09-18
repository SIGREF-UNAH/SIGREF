# syntax=docker/dockerfile:1.7
# ============================================================================
# CAMBIOS APLICADOS (Codex, 2026-09-17)
# - Restore reproducible y cacheable: se copian todos los proyectos referenciados.
# - Ejecucion no root explicita, un unico puerto HTTP y diagnostico remoto apagado.
# - La imagen final contiene unicamente los artefactos publicados.
# ============================================================================

# Imagen runtime reducida: no contiene SDK ni herramientas de compilacion.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
WORKDIR /app

# La API escucha internamente en 8080. La publicacion al host se define en Compose.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

# Etapa de construccion: queda fuera de la imagen final.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Se copian primero todos los .csproj transitivos para reutilizar la capa de restore.
COPY ["SIGREF.API/SIGREF.API.csproj", "SIGREF.API/"]
COPY ["SIGREF.API.ServiceDefaults/SIGREF.API.ServiceDefaults.csproj", "SIGREF.API.ServiceDefaults/"]
COPY ["SIGREF.Common/SIGREF.Common.csproj", "SIGREF.Common/"]
COPY ["SIGREF.Core/SIGREF.Core.csproj", "SIGREF.Core/"]
COPY ["SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj", "SIGREF.Hangfire.Worker/"]
COPY ["SIGREF.Infrastructure.Keycloak/SIGREF.Infrastructure.Keycloak.csproj", "SIGREF.Infrastructure.Keycloak/"]
COPY ["SIGREF.Infrastructure.Persistence/SIGREF.Infrastructure.Persistence.csproj", "SIGREF.Infrastructure.Persistence/"]
COPY ["SIGREF.Infrastructure.Reporting/SIGREF.Infrastructure.Reporting.csproj", "SIGREF.Infrastructure.Reporting/"]

# La cache de NuGet acelera builds repetidos sin incorporarse a la imagen resultante.
RUN --mount=type=cache,id=nuget-api,target=/root/.nuget/packages \
    dotnet restore "SIGREF.API/SIGREF.API.csproj"

COPY . .
WORKDIR /src/SIGREF.API
# La API referencia al Worker, que tambien aporta appsettings.json. Esta opcion
# conserva la configuracion del proyecto raiz sin alterar el .csproj del usuario.
RUN --mount=type=cache,id=nuget-api,target=/root/.nuget/packages \
    dotnet publish "SIGREF.API.csproj" \
      --configuration "$BUILD_CONFIGURATION" \
      --output /app/publish \
      --no-restore \
      /p:UseAppHost=false \
      /p:ErrorOnDuplicatePublishOutputFiles=false

FROM base AS final
WORKDIR /app

# Los artefactos pertenecen al usuario predefinido de las imagenes .NET.
COPY --chown=$APP_UID:$APP_UID --from=build /app/publish .
USER $APP_UID

ENTRYPOINT ["dotnet", "SIGREF.API.dll"]
