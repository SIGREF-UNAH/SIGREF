using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Catalogs;

namespace SIGREF.API.Database.Configurations;

public class HealthServicesPackagesNnConfiguration : IEntityTypeConfiguration<HealthServicesPackagesNnEntity >
{
    public void Configure(EntityTypeBuilder<HealthServicesPackagesNnEntity> builder)
    {
        // ============================
        //          TABLE
        // ============================
        builder.ToTable("health_services_packages_nn",
            t =>
            {
                t.HasComment(
                    "Tabla puente que relaciona los servicios de salud con los paquetes disponibles en SIGREF.");
            });

        // ============================
        //      PRIMARY KEY
        // ============================
        builder.HasKey(x => x.Id);

        // ============================
        //      COLUMN MAPPINGS
        // ============================

        builder.Property(x => x.HealthServiceId)
            .HasColumnName("health_service_id")
            .IsRequired()
            .HasComment("ID del servicio de salud que pertenece al paquete.");

        builder.Property(x => x.PackageId)
            .HasColumnName("package_id")
            .IsRequired()
            .HasComment("ID del paquete que agrupa servicios.");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasComment("Indica si la relación entre servicio y paquete está activa.");

        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("ID del usuario (UserLink) que creó la relación.");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("ID del usuario (UserLink) que modificó por última vez la relación.");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación del registro (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización del registro (UTC).");

        // ============================
        //      RELACIONES
        // ============================

        builder.HasOne(x => x.HealthService)
            .WithMany()
            .HasForeignKey(x => x.HealthServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Package)
            .WithMany()
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================
        //          ÍNDICES
        // ============================

        builder.HasIndex(x => x.HealthServiceId)
            .HasDatabaseName("idx_hspnn_service");

        builder.HasIndex(x => x.PackageId)
            .HasDatabaseName("idx_hspnn_package");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("idx_hspnn_active");

        builder.HasIndex(x => x.CreatedById)
            .HasDatabaseName("idx_hspnn_created_by");

        builder.HasIndex(x => x.UpdatedById)
            .HasDatabaseName("idx_hspnn_updated_by");

        // Índice único: NO permite duplicar pares servicio-paquete
        builder.HasIndex(x => new { x.HealthServiceId, x.PackageId })
            .IsUnique()
            .HasDatabaseName("idx_hspnn_unique_service_package");
    }
}