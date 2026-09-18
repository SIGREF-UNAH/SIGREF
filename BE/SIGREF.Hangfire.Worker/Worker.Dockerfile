# syntax=docker/dockerfile:1.7
# ============================================================================
# CAMBIOS APLICADOS (Codex, 2026-09-17)
# - El Worker usa la imagen .NET runtime, no ASP.NET Core, y no expone puertos.
# - Restore cacheable con todas las referencias transitivas y ejecucion no root.
# - Solo se instala la dependencia nativa necesaria antes de crear la imagen final.
# ============================================================================

# El Worker no hospeda HTTP; runtime reduce tamano y superficie de ataque.
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app

# QuestPDF y autenticacion integrada pueden requerir estas bibliotecas en Alpine.
RUN apk add --no-cache krb5-libs
ENV DOTNET_EnableDiagnostics=0 \
    DOTNET_RUNNING_IN_CONTAINER=true

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Referencias directas y transitivas para que restore sea valido y cacheable.
COPY ["SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj", "SIGREF.Hangfire.Worker/"]
COPY ["SIGREF.API.ServiceDefaults/SIGREF.API.ServiceDefaults.csproj", "SIGREF.API.ServiceDefaults/"]
COPY ["SIGREF.Common/SIGREF.Common.csproj", "SIGREF.Common/"]
COPY ["SIGREF.Core/SIGREF.Core.csproj", "SIGREF.Core/"]
COPY ["SIGREF.Infrastructure.Keycloak/SIGREF.Infrastructure.Keycloak.csproj", "SIGREF.Infrastructure.Keycloak/"]
COPY ["SIGREF.Infrastructure.Persistence/SIGREF.Infrastructure.Persistence.csproj", "SIGREF.Infrastructure.Persistence/"]
COPY ["SIGREF.Infrastructure.Reporting/SIGREF.Infrastructure.Reporting.csproj", "SIGREF.Infrastructure.Reporting/"]

RUN --mount=type=cache,id=nuget-worker,target=/root/.nuget/packages \
    dotnet restore "SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj"

COPY . .
WORKDIR /src/SIGREF.Hangfire.Worker
RUN --mount=type=cache,id=nuget-worker,target=/root/.nuget/packages \
    dotnet publish "SIGREF.Hangfire.Worker.csproj" \
      --configuration "$BUILD_CONFIGURATION" \
      --output /app/publish \
      --no-restore \
      /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --chown=$APP_UID:$APP_UID --from=build /app/publish .
USER $APP_UID

ENTRYPOINT ["dotnet", "SIGREF.Hangfire.Worker.dll"]
