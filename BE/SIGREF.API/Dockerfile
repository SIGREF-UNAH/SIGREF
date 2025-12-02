FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto desde el contexto correcto
COPY ["SIGREF.API/SIGREF.API.csproj", "SIGREF.API/"]
COPY ["SIGREF.API.ServiceDefaults/SIGREF.API.ServiceDefaults.csproj", "SIGREF.API.ServiceDefaults/"]

# Restaurar dependencias
RUN dotnet restore "SIGREF.API/SIGREF.API.csproj"

# Copiar el resto del código
COPY . .

WORKDIR "/src/SIGREF.API"
RUN dotnet build "SIGREF.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SIGREF.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIGREF.API.dll"]