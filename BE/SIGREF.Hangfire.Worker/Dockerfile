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
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app

# 2. SDK DE COMPILACIÓN (Alpine)
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 3. OPTIMIZACIÓN DE CACHÉ (Restore)
# Copiamos archivos de proyecto de forma aislada para aprovechar la caché de Docker.
COPY ["SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj", "SIGREF.Hangfire.Worker/"]
COPY ["SIGREF.ServicesDefault/SIGREF.ServicesDefault.csproj", "SIGREF.ServicesDefault/"]

# Uso de montajes de caché para NuGet para acelerar builds repetitivos.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "SIGREF.Hangfire.Worker/SIGREF.Hangfire.Worker.csproj"

# 4. COMPILACIÓN DEL CÓDIGO
COPY . .
WORKDIR "/src/SIGREF.Hangfire.Worker"
RUN dotnet build "SIGREF.Hangfire.Worker.csproj" -c Release -o /app/build

# 5. PUBLICACIÓN AVANZADA
FROM build AS publish
# PublishReadyToRun: Pre-compila el código a código nativo para reducir 
# el tiempo de JIT (Just-In-Time) durante el arranque del Worker.
RUN dotnet publish "SIGREF.Hangfire.Worker.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# 6. IMAGEN FINAL (Ultra liviana)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

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