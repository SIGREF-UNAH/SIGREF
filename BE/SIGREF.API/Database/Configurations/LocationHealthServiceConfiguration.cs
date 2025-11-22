using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Catalogs;

namespace SIGREF.API.Database.Configurations;

public class LocationHealthServiceConfiguration : IEntityTypeConfiguration<LocationHealthServiceEntity>
{
    public void Configure(EntityTypeBuilder<LocationHealthServiceEntity> builder)
    {
        // ===============================
        //       TABLE NAME
        // ===============================
        builder.ToTable("locations_health_services",
            t =>
            {
                t.HasComment(
                    "Relación N:N entre Locations y Health Services. Define qué servicios están disponibles en cada ubicación.");
            });


        // ===============================
        //       PRIMARY KEY
        // ===============================
        builder.HasKey(e => e.Id);

        // ===============================
        //       PROPERTIES
        // ===============================
        builder.Property(e => e.LocationId)
            .IsRequired()
            .HasColumnName("location_id");

        builder.Property(e => e.ServiceId)
            .IsRequired()
            .HasColumnName("service_id");

        // ===============================
        //       RELACIONES
        // ===============================
        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Service)
            .WithMany()
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===============================
        //       ÍNDICES 
        // ===============================
        builder.HasIndex(e => new { e.LocationId, e.ServiceId })
            .IsUnique();

        builder.HasIndex(e => e.LocationId);

        builder.HasIndex(e => e.ServiceId);
    }
}