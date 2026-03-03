# ============================================================================
# MÓDULO: API Principal (Backend)
# PROYECTO: SIGREF (.NET 9)
# ----------------------------------------------------------------------------
# AUTORES (GitHub): 
#   - @erickArita (Erick Marley Arita) -> Estructura y lógica de capas
#   - @TETvega (Héctor Rene Martínez Vega) -> Arquitectura de servicios
# CO-AUTOR DE OPTIMIZACIÓN Y VALIDACIÓN: 
#   - Gemini 3 Flash (Google AI) -> Estrategia Chiseled & BuildKit Cache
# ----------------------------------------------------------------------------
# PROPÓSITO: Construcción de imagen de alta eficiencia, seguridad y 
#            rendimiento optimizado para despliegues en la nube.
# ============================================================================

# ----------------------------------------------------------------------------
# 1. ETAPA DE EJECUCIÓN (Runtime)
# ----------------------------------------------------------------------------
# Utiliza "Ubuntu Chiseled", una imagen ultra-minimalista sin shell ni gestor 
# de paquetes. Reduce la superficie de ataque y el tamaño final (~50MB vs ~200MB).
# Ref: https://devblogs.microsoft.com/dotnet/announcing-dotnet-chiseled-containers/
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ----------------------------------------------------------------------------
# 2. ETAPA DE CONSTRUCCIÓN (SDK)
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# OPTIMIZACIÓN DE CAPAS: Copiamos solo los archivos de proyecto (.csproj).
# Esto permite que Docker cachee el paso de 'restore'. Si el código cambia 
# pero las dependencias no, este paso se salta, ahorrando minutos de descarga.
COPY ["SIGREF.API/SIGREF.API.csproj", "SIGREF.API/"]
COPY ["SIGREF.API.ServiceDefaults/SIGREF.API.ServiceDefaults.csproj", "SIGREF.API.ServiceDefaults/"]

# BUILDKIT CACHE: Usamos un montaje de caché para la carpeta de NuGet.
# Evita re-descargar paquetes incluso si la caché de capas de Docker se invalida.
# Requiere Docker BuildKit habilitado.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "SIGREF.API/SIGREF.API.csproj"

# Copiamos el resto del código fuente una vez restauradas las dependencias.
COPY . .
WORKDIR "/src/SIGREF.API"

# PUBLICACIÓN ATÓMICA: 'dotnet publish' realiza implícitamente el build.
# Al hacerlo en un solo paso con caché, generamos menos metadatos de capas intermedias.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "SIGREF.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ----------------------------------------------------------------------------
# 3. ETAPA FINAL (Imagen de Producción)
# ----------------------------------------------------------------------------
FROM base AS final
WORKDIR /app

# Copiamos solo los binarios compilados desde la etapa de build.
COPY --from=build /app/publish .

# SEGURIDAD: Las imágenes chiseled no tienen usuario root activo por defecto.
# Se ejecutan con un usuario 'app' de bajos privilegios pre-configurado.
ENTRYPOINT ["dotnet", "SIGREF.API.dll"]

# ----------------------------------------------------------------------------
# REFERENCIAS TÉCNICAS:
# - Docker BuildKit Mounts: https://docs.docker.com/build/guide/mounts/
# - .NET Container Best Practices: https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
# - Ubuntu Chiseled for .NET: https://github.com/dotnet/dotnet-docker/blob/main/samples/chiseled.md
# ----------------------------------------------------------------------------