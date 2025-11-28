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

        // ===============================
        //           ÍNDICES
        // ===============================
        builder.HasIndex(e => new { e.LocationId, e.PackageId })
            .IsUnique();

        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.PackageId);
    }
}