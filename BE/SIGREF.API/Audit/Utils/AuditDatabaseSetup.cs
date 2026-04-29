using MongoDB.Driver;
using SIGREF.API.Audit.Dto;

namespace SIGREF.API.Audit.Utils;

/// <summary>
/// Se encarga de la configuración inicial de la colección de auditoría en MongoDB,
/// garantizando que las consultas futuras sean de alto rendimiento.
/// </summary>
public static class AuditDatabaseSetup
{
    public static async Task EnsureIndexesAsync(IMongoCollection<AuditLog> collection, ILogger logger, CancellationToken ct)
    {
        try
        {
            var builder = Builders<AuditLog>.IndexKeys;
            var indexModels = new List<CreateIndexModel<AuditLog>>();

            // 1. TraceId (Búsqueda exacta)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Ascending(x => x.TraceId),
                new CreateIndexOptions { Background = true }
            ));

            // 2. Fecha (Ordenamiento global de más reciente a más antiguo)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Descending(x => x.Timestamp),
                new CreateIndexOptions { Background = true }
            ));

            // 3. Historial por Recurso (Índice Compuesto)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Ascending(x => x.ResourceType).Ascending(x => x.ResourceId),
                new CreateIndexOptions { Background = true }
            ));

            // 4. Auditoría por Usuario (Índice Compuesto: Qué hizo alguien y cuándo)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Ascending(x => x.UserId).Descending(x => x.Timestamp),
                new CreateIndexOptions { Background = true }
            ));

            // 5. Análisis de Endpoints (Índice Compuesto: Rutas específicas)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Ascending(x => x.Endpoint).Ascending(x => x.HttpMethod),
                new CreateIndexOptions { Background = true }
            ));

            // 6. Análisis de Acciones (Ej: Buscar todos los DELETE recientes)
            indexModels.Add(new CreateIndexModel<AuditLog>(
                builder.Ascending(x => x.Action).Descending(x => x.Timestamp),
                new CreateIndexOptions { Background = true }
            ));

            // Ejecutar la creación en bloque
            await collection.Indexes.CreateManyAsync(indexModels, ct);
            
            logger.LogInformation("Los índices de auditoría en MongoDB han sido validados/creados con éxito.");
        }
        catch (Exception ex)
        {
            // Atrapamos el error para no tumbar la aplicación, pero lo registramos
            logger.LogWarning(ex, "Advertencia: Ocurrió un problema al verificar los índices de auditoría en MongoDB.");
        }
    }
}