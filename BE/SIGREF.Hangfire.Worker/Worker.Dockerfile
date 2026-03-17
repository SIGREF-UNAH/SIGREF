# ============================================================================
# MÓDULO: Worker de Tareas en Segundo Plano (Hangfire)
# PROYECTO: SIGREF (.NET 9)
# ----------------------------------------------------------------------------
# AUTOR (GitHub): 
#   - @TETvega (Héctor Rene Martínez Vega) -> Arquitectura y lógica de procesos
# CO-AUTOR DE OPTIMIZACIÓN: 
#   - Gemini 3 Flash (Google AI) -> Estrategia de compilación ReadyToRun
# ----------------------------------------------------------------------------
# PROPÓSITO: Servicio de alta eficiencia para procesamiento asíncrono, 
#            optimizado para bajo consumo de recursos y seguridad Alpine.
# ============================================================================

# 1. IMAGEN DE EJECUCIÓN (Runtime)
# Usamos 'runtime-alpine' para reducir el peso al mínimo absoluto al no 
# requerir las librerías de servidor web (ASP.NET Core).
# 1. IMAGEN BASE DE RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
# 2. SDK DE COMPILACIÓN (Alpine)
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 3. OPTIMIZACIÓN DE CACHÉ (Restore)
# Copiamos archivos de proyecto de forma aislada para aprovechar la caché de Docker.
# 3. RESTORE AISLADO — solo .csproj para máximo cache de capas
COPY ["SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj",                         "SIGREF.Hangfire.Worker/"]
COPY ["SIGREF.API.ServiceDefaults/SIGREF.API.ServiceDefaults.csproj",                 "SIGREF.API.ServiceDefaults/"]
COPY ["SIGREF.Infrastructure.Reporting/SIGREF.Infrastructure.Reporting.csproj",       "SIGREF.Infrastructure.Reporting/"]
COPY ["SIGREF.Infrastructure.Persistence/SIGREF.Infrastructure.Persistence.csproj",   "SIGREF.Infrastructure.Persistence/"]
COPY ["SIGREF.Common/SIGREF.Common.csproj",                                           "SIGREF.Common/"]
# Uso de montajes de caché para NuGet para acelerar builds repetitivos.

RUN --mount=type=cache,id=nuget-worker,target=/root/.nuget/packages \
    dotnet restore "SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj"

# 4. COPIAR SOLO LOS PROYECTOS NECESARIOS (no todo el repo)
COPY SIGREF.Hangfire.Worker/            SIGREF.Hangfire.Worker/
COPY SIGREF.API.ServiceDefaults/        SIGREF.API.ServiceDefaults/
COPY SIGREF.Infrastructure.Reporting/  SIGREF.Infrastructure.Reporting/
COPY SIGREF.Infrastructure.Persistence/ SIGREF.Infrastructure.Persistence/
COPY SIGREF.Common/                     SIGREF.Common/

# 4. COMPILACIÓN DEL CÓDIGO
COPY . .
WORKDIR "/src/SIGREF.Hangfire.Worker"

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "SIGREF.Hangfire.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# 6. IMAGEN FINAL (Ultra liviana)
FROM base AS final
WORKDIR /app
RUN apk add --no-cache krb5-libs
COPY --from=build /app/publish .


# SEGURIDAD: Uso de usuario no root (definido en imágenes .NET por defecto como $APP_UID)
# para prevenir escalamiento de privilegios en el contenedor.
USER $APP_UID
ENTRYPOINT ["dotnet", "SIGREF.Hangfire.Worker.dll"]

# ----------------------------------------------------------------------------
# REFERENCIAS TÉCNICAS:
# - .NET Runtime vs ASP.NET Core: https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
# - ReadyToRun Compilation: https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run
# - Alpine Security Hardening: https://wiki.alpinelinux.org/wiki/Security
# ----------------------------------------------------------------------------
