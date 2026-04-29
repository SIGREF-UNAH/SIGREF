using MongoDB.Driver;
using SIGREF.API.Audit.Dto;
using SIGREF.API.Audit.Middleware.Quee;
using SIGREF.API.Audit.Utils;

namespace SIGREF.API.Audit.Middleware.worker;

public class AuditWorker : BackgroundService
{
    private readonly IAuditQueue _queue;
    private readonly IMongoCollection<AuditLog> _collection;
    private readonly ILogger<AuditWorker> _logger;

    public AuditWorker(IAuditQueue queue, IMongoClient mongoClient, IConfiguration config, ILogger<AuditWorker> logger)
    {
        _queue = queue;
        _logger = logger;

        var dbName = config["MongoDbSettings:DatabaseName"] ?? "SIGREF_Audit";
        var database = mongoClient.GetDatabase(dbName);
        _collection = database.GetCollection<AuditLog>("Logs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditWorker iniciando...");

        // Delegamos la configuración de índices a la clase especializada
        await AuditDatabaseSetup.EnsureIndexesAsync(_collection, _logger, stoppingToken);

        _logger.LogInformation("AuditWorker listo. Escuchando eventos en la cola...");

        await foreach (var log in _queue.ReadAllAsync(stoppingToken))
        {
            try 
            {
                await _collection.InsertOneAsync(log, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico guardando auditoría. TraceId: {TraceId}", log.TraceId);
            }
        }
    }
}