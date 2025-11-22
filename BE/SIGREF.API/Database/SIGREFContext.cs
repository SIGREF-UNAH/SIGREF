using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.Billing;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Database.Entity.Catalogs;
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
    public DbSet<UserLinkEntity> UserLinks { get; set; } = default!;
    public DbSet<HospitalPropertiesEntity> HospitalProperties { get; set; } = default!;

    // --- Catálogos ---
    public DbSet<HealthServicesEntity> HealthServices { get; set; } = default!;
    public DbSet<HealthServicePackagesEntity> HealthServicePackages { get; set; } = default!;
    public DbSet<HealthServicesPackagesNnEntity> HealthServicesPackagesNn { get; set; } = default!;
    public DbSet<LocationEntity> Locations { get; set; } = default!;
    public DbSet<LocationHealthServiceEntity> LocationHealthServices { get; set; } = default!;
    public DbSet<LocationHealthServicePackageEntity> LocationHealthServicePackages { get; set; } = default!;

    // --- Facturación ---
    public DbSet<InvoiceEntity> Invoices { get; set; } = default!;
    public DbSet<InvoiceSerieEntity> InvoiceSeries { get; set; } = default!;
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = default!;

    // --- Caja / Turnos ---
    public DbSet<ShiftEntity> Shifts { get; set; } = default!;
    public DbSet<CashierSessionEntity> CashierSessions { get; set; } = default!;

    // --- Reportes ---
    public DbSet<ReportHistoryEntity> ReportHistory { get; set; } = default!;


    // ============================
    //         MODEL BUILDING
    // ============================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente TODAS las configuraciones
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SIGREFContext).Assembly);
    }
}
