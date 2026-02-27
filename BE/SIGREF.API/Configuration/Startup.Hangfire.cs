

using Hangfire;
using Hangfire.PostgreSql;

namespace SIGREF.API;
public partial class Startup
{
    private void AddHangfire(IServiceCollection services)
    {
        var hangfireConn = _configuration.GetConnectionString("hangfire");
        if (string.IsNullOrWhiteSpace(hangfireConn))
        {
            throw new InvalidOperationException(
                "Connection string 'hangfire' no está configurada. Define ConnectionStrings__hangfire.");
        }

        services.AddHangfire((sp, cfg) =>
        {
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UsePostgreSqlStorage(
                    bootstrap =>
                    {
                        //  nuevo patrón (evita el obsoleto)
                        bootstrap.UseNpgsqlConnection(hangfireConn);
                    },
                    new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(5),
                        InvisibilityTimeout = TimeSpan.FromMinutes(5),
                        DistributedLockTimeout = TimeSpan.FromMinutes(1),
                    });
        });

        var serverEnabled = _configuration.GetValue<bool>("Hangfire:ServerEnabled");
        if (serverEnabled)
        {
            services.AddHangfireServer(opt =>
            {
                opt.ServerName = $"sigref-api-{Environment.MachineName}";
            });
        }
    }

    private void UseHangfireDashboard(IApplicationBuilder app)
    {
        var dashboardEnabled = _configuration.GetValue<bool>("Hangfire:DashboardEnabled");
        if (!dashboardEnabled) return;

        // Si aún no has agregado auth filter, lo dejamos abierto por ahora.
        // Luego lo cerramos con roles (admin/ti).
        app.UseHangfireDashboard("/hangfire");
    }
}

