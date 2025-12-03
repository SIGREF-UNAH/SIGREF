using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.Billing;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Database.Entity.Catalogs;
using SIGREF.API.Database.Entity.Dashboard;
using SIGREF.API.Database.Entity.Files;
using SIGREF.API.Database.Entity.Reports;

namespace SIGREF.API.Database;

public class SIGREFContext : DbContext
{
    public SIGREFContext(DbContextOptions<SIGREFContext> options) : base(options)
    {
    }

    // ============================
    //            DBSETS
    // ============================

    // --- Administración ---
    public DbSet<HospitalPropertiesEntity> HospitalProperties { get; set; } = default!;

    public DbSet<MediaFileEntity> MediaFiles { get; set; } = default!;
    // --- Catálogos ---
    public DbSet<HealthService> HealthServices { get; set; } = default!;

    // --- Facturación ---
    public DbSet<InvoiceEntity> Invoices { get; set; } = default!;
    public DbSet<InvoiceSerieEntity> InvoiceSeries { get; set; } = default!;
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = default!;

    // --- Caja / Turnos ---
    public DbSet<ShiftEntity> Shifts { get; set; } = default!;
    public DbSet<CashierSessionEntity> CashierSessions { get; set; } = default!;

    // --- Reportes ---
    public DbSet<ReportHistoryEntity> ReportHistory { get; set; } = default!;

    // --- Vistas Materializadas ---
    public DbSet<DashboardFact> DashboardFacts { get; set; } = default!;
    // ============================
    //         MODEL BUILDING
    // ============================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente TODAS las configuraciones
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SIGREFContext).Assembly);
        // =======================
        // MATERIALIZED VIEW MAP
        // =======================
        modelBuilder.Entity<DashboardFact>(entity =>
        {
            entity.ToView("mv_dashboard_facts");  // nombre exacto de la MV en Postgres
            entity.HasNoKey();                    // obligatorio para VIEW o MV
        });

    }
}
