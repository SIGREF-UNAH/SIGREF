using Hangfire;
using Hangfire.PostgreSql;
using SIGREF.API.Services.Hangfire;

namespace SIGREF.API;

public partial class Startup
{
    private void AddHangfire(IServiceCollection services)
    {
        // Aspire inyecta automáticamente 'ConnectionStrings__hangfire'
        var hangfireConn = _configuration.GetConnectionString("hangfire");

        if (string.IsNullOrWhiteSpace(hangfireConn))
        {
            throw new InvalidOperationException(
                "La cadena de conexión 'hangfire' no se encontró. Verifique la orquestación en el AppHost.");
        }

        // Servicios de lógica de negocio para tareas
        services.AddScoped<HangfireTestService>();

        // --------------------------------------------------------------------
        // CONFIGURACIÓN DEL STORAGE (PostgreSQL)
        // --------------------------------------------------------------------
        services.AddHangfire((sp, cfg) =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UsePostgreSqlStorage(
                    bootstrap => bootstrap.UseNpgsqlConnection(hangfireConn),
                    new PostgreSqlStorageOptions
                    {
                        // @TETvega: Solo el que tiene el Server debería preparar el esquema 
                        // para evitar bloqueos, pero dejarlo en true es seguro en desarrollo.
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(5),
                        InvisibilityTimeout = TimeSpan.FromMinutes(5),
                        DistributedLockTimeout = TimeSpan.FromMinutes(1),
                    });
        });

        // --------------------------------------------------------------------
        // LÓGICA DE PROCESAMIENTO (SERVER)
        // --------------------------------------------------------------------
        // Sincronizado con AppHost: Hangfire__IsEmbedded
        // Si IsEmbedded es true, este contenedor PROCESA las tareas.
        var isEmbedded = _configuration.GetValue<bool>("Hangfire:IsEmbedded", true);

        if (isEmbedded)
        {
            services.AddHangfireServer(opt =>
            {
                opt.ServerName = $"SIGREF-NODE-{Environment.MachineName}";

                // Lógica Pro: 
                // Mínimo 2 hilos (para que no se trabe)
                // Máximo 20 hilos (para no agotar las conexiones a la base de datos)
                int calculatedWorkers = Environment.ProcessorCount * 2;
                opt.WorkerCount = Math.Clamp(calculatedWorkers, 2, 20);
            });
        }
    }

    private void UseHangfireDashboard(IApplicationBuilder app)
    {
        // Sincronizado con AppHost: Hangfire__EnableDashboard
        var dashboardEnabled = _configuration.GetValue<bool>("Hangfire:EnableDashboard", true);

        if (!dashboardEnabled) return;

        // NOTA: 
        // En el futuro, aquí agregaremos el DashboardOptions con un AuthorizationFilter
        // para que solo los usuarios con rol 'ADMIN' o 'TI' entren.
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "SIGREF - Panel de Tareas",
            // AppPath = "/dashboard" // si se quiere volver al dashboard 
        });
    }
}