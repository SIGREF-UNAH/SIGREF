using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Catalogs;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class HealthServicesConfiguration : BaseEntityConfiguration<HealthService>
{
    public override void Configure(EntityTypeBuilder<HealthService> builder)
    {
        
        base.Configure(builder);
        builder.ToTable("health_services",
            t =>
            {
                t.HasComment(
                    "Catálogo de servicios de salud registrados en SIGREF. Sincronizado parcialmente con FHIR HealthcareService.");
            });
        
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
            // https://fhir.hl7.org/fhir/datatypes.html#:~:text=JSON%20Definition-,id,Regex%3A%20%5BA%2DZa%2Dz0%2D9%5C%2D%5C.%5D%7B1%2C64%7D,-XML%20Definition
            .IsUnicode(false)
            .IsRequired()
            .HasComment("ID lógico del recurso HealthcareService según estándar FHIR R4 (max 64 chars).");
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
            .HasPrecision(18, 2) 
            .HasComment("Precio asignado al servicio para facturación (máximo 18 dígitos, 2 decimales).");
        

        // builder.Property(x => x.LastSync)
        //     .HasColumnName("last_sync")
        //     .HasComment("Fecha de última sincronización con FHIR o procesos automáticos.");

        
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