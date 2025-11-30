using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Catalogs;

namespace SIGREF.API.Database.Configurations;

public class LocationHealthServicePackageConfiguration : IEntityTypeConfiguration<LocationHealthServicePackageEntity>
{
    public void Configure(EntityTypeBuilder<LocationHealthServicePackageEntity> builder)
    {
        // ===============================
        //           TABLE
        // ===============================
        builder.ToTable("locations_health_services_package",
            t =>
            {
                t.HasComment(
                    "Relación N:N entre Locations y Health Service Packages. Define qué paquetes están disponibles en cada ubicación.");
            });

        // ===============================
        //           PRIMARY KEY
        // ===============================
        builder.HasKey(e => e.Id);

        // ===============================
        //           PROPERTIES
        // ===============================
        builder.Property(e => e.LocationId)
            .IsRequired()
            .HasColumnName("location_id");

        builder.Property(e => e.PackageId)
            .IsRequired()
            .HasColumnName("package_id");

        // ===============================
        //           RELACIONES
        // ===============================

        builder.HasOne(e => e.Package)
            .WithMany()
            .HasForeignKey(e => e.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ============================
        //          AUDITORÍA
        // ============================
        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("ID del usuario que creó el registro.");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("ID del usuario que realizó la última actualización.");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación del turno (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización (UTC).");

        // ===============================
        //           ÍNDICES
        // ===============================
        builder.HasIndex(e => new { e.LocationId, e.PackageId })
            .IsUnique();

        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.PackageId);
    }
}