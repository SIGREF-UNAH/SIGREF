using SIGREF.Hangfire.Worker;
using Hangfire;
using Hangfire.PostgreSql;
using SIGREF.Infrastructure.Reporting; 
var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

//  Obtener cadena de conexión (Inyectada por Aspire)
var hangfireConn = configuration.GetConnectionString("hangfire");
builder.AddNpgsqlDataSource("sigref"); 
builder.AddNpgsqlDataSource("hapi");
if (string.IsNullOrWhiteSpace(hangfireConn))
{
    throw new InvalidOperationException(
        "Error Crítico: Connection string 'hangfire' no inyectada por el AppHost.");
}

// 2. Configurar el Storage (Igual que en la API para mantener compatibilidad)
builder.Services.AddHangfire((sp, cfg) =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UsePostgreSqlStorage(
            bootstrap => bootstrap.UseNpgsqlConnection(hangfireConn),
            new PostgreSqlStorageOptions
            {
                PrepareSchemaIfNecessary = true, // El worker puede preparar el esquema
                QueuePollInterval = TimeSpan.FromSeconds(5)
            });
});

// REGISTRO de la Libreria comun del API
// 
builder.AddReportingInfrastructure();


// Lógica de Activación del Servidor
// Sincronizamos con la variable que enviamos desde el AppHost: Hangfire__IsWorkerOnly
// O simplemente lo encendemos siempre, ya que si este contenedor existe, es para procesar.
var isWorkerOnly = configuration.GetValue<bool>("Hangfire:IsWorkerOnly", true);

if (isWorkerOnly)
{
    builder.Services.AddHangfireServer(opt =>
    {
        opt.ServerName = $"SIGREF-WORKER-{Environment.MachineName}";
        
        // OPTIMIZACIÓN ELÁSTICA:
        // Como este contenedor es SOLAMENTE para procesar
        // Mínimo 4 hilos, máximo 30.
        int calculatedWorkers = Environment.ProcessorCount * 3; 
        opt.WorkerCount = Math.Clamp(calculatedWorkers, 4, 30);
    });
}

// Servicio en segundo plano para otras tareas de monitoreo del worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();