using Hangfire;
using Hangfire.PostgreSql;

namespace SIGREF.Hangfire.Worker;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddWorkerHangfire(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("hangfire") 
                               ?? throw new InvalidOperationException("Connection string 'hangfire' no inyectada.");

        builder.Services.AddHangfire((sp, cfg) =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(bootstrap => bootstrap.UseNpgsqlConnection(connectionString));
        });

        // Configuración del servidor optimizada
        builder.Services.AddHangfireServer(opt =>
        {
            opt.ServerName = $"SIGREF-WORKER-{Environment.MachineName}";
            int workers = Environment.ProcessorCount * 3;
            opt.WorkerCount = Math.Clamp(workers, 4, 30);
        });

        return builder;
    }
}