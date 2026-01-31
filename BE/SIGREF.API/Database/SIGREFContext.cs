using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.Billing;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Database.Entity.Catalogs;
using SIGREF.API.Database.Entity.Dashboard;
using SIGREF.API.Database.Entity.Files;
using SIGREF.API.Database.Entity.Reports;
using SIGREF.API.Dtos.Dashboard;

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
        
        modelBuilder.Entity<ServiceUsageRow>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null); // no existe tabla/view física
        });
        modelBuilder.Entity<ShiftIncomeRow>(eb =>
        {
            eb.HasNoKey();      
            eb.ToView(null);    //  no está ligado a una tabla/view real para migraciones
        });
        modelBuilder.Entity<LocationIncomeRow>(eb =>
        {
            eb.HasNoKey();      
            eb.ToView(null);    //  no está ligado a una tabla/view real para migraciones
        });
        
    }
}