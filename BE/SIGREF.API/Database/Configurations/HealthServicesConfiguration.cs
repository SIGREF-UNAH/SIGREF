using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Catalogs;

namespace SIGREF.API.Database.Configurations;

public class HealthServicesConfiguration : IEntityTypeConfiguration<HealthService>
{
    public void Configure(EntityTypeBuilder<HealthService> builder)
    {
        builder.ToTable("health_services",
            t =>
            {
                t.HasComment(
                    "Catálogo de servicios de salud registrados en SIGREF. Sincronizado parcialmente con FHIR HealthcareService.");
            });

        // ============================
        //       PRIMARY KEY
        // ============================
        builder.HasKey(x => x.Id);

        // ============================
        //       COLUMN MAPPINGS
        // ============================

        // builder.Property(x => x.Name)
        //     .HasColumnName("name")
        //     .HasMaxLength(150)
        //     .IsRequired()
        //     .HasComment("Nombre del servicio de salud.");

        // builder.Property(x => x.Description)
        //     .HasColumnName("description")
        //     .HasMaxLength(300)
        //     .HasComment("Descripción del servicio.");

        builder.Property(x => x.HealthServiceFhirId)
            .HasColumnName("health_service_id_fhir")
            .HasMaxLength(64)
            .IsRequired()
            .HasComment("ID del recurso HealthcareService en FHIR.");
        //
        // builder.Property(x => x.Abbreviation)
        //     .HasColumnName("abbreviation")
        //     .HasMaxLength(5)
        //     .HasComment("Abreviación corta (0–5 caracteres).");
        //
        // builder.Property(x => x.IsActive)
        //     .HasColumnName("is_active")
        //     .IsRequired()
        //     .HasComment("Indica si el servicio está activo.");

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasComment("Precio asignado al servicio para facturación.");

        // ============================
        //       AUDITORÍA
        // ============================

        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("Usuario que creó el registro.");
 

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización (UTC).");

        // builder.Property(x => x.LastSync)
        //     .HasColumnName("last_sync")
        //     .HasComment("Fecha de última sincronización con FHIR o procesos automáticos.");

        // ============================
        //          RELACIONES
        // ============================

        



        // ============================
        //          ÍNDICES
        // ============================

        builder.HasIndex(x => x.HealthServiceFhirId)
            .IsUnique()
            .HasDatabaseName("idx_health_services_fhir");

        // builder.HasIndex(x => x.Name)
        //     .HasDatabaseName("idx_health_services_name");
        //
        // builder.HasIndex(x => x.IsActive)
        //     .HasDatabaseName("idx_health_services_active");
        //
        // builder.HasIndex(x => new { x.Name, x.IsActive })
        //     .HasDatabaseName("idx_health_services_name_active");
        //
        // builder.HasIndex(x => x.LastSync)
        //     .HasDatabaseName("idx_health_services_last_sync");
    }
}