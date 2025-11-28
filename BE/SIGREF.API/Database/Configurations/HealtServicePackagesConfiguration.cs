using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Catalogs;

namespace SIGREF.API.Database.Configurations;

public class HealtServicePackagesConfiguration : IEntityTypeConfiguration<HealthServicePackagesEntity>
{
    public void Configure(EntityTypeBuilder<HealthServicePackagesEntity> builder)
    {
        // ============================
        //        TABLE
        // ============================
        builder.ToTable("health_services_package",
            t => { t.HasComment("Catálogo de paquetes de servicios de salud administrados en SIGREF."); });

        // ============================
        //       PRIMARY KEY
        // ============================
        builder.HasKey(x => x.Id);

        // ============================
        //       PROPERTIES
        // ============================

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired()
            .HasComment("Nombre del paquete de servicios.");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .HasComment("Descripción general del paquete de servicios.");

        builder.Property(x => x.Abbreviation)
            .HasColumnName("abbreviation")
            .HasMaxLength(5)
            .HasComment("Abreviación del paquete (0 a 5 caracteres).");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasComment("Indica si el paquete está activo para facturación o listados administrativos.");

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasComment("Precio asignado al paquete. Se usa para facturación y reportes.");

        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("ID del usuario (UserLink) que creó el registro.");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("ID del usuario (UserLink) que modificó por última vez el registro.");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación del registro (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización (UTC).");

        builder.Property(x => x.LastSync)
            .HasColumnName("last_sync")
            .HasComment("Fecha de última sincronización realizada por procesos automáticos.");

        // ============================
        //        RELACIONES
        // ============================



        // ============================
        //          ÍNDICES
        // ============================

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("idx_hspackage_name");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("idx_hspackage_active");

        builder.HasIndex(x => x.Price)
            .HasDatabaseName("idx_hspackage_price");

        builder.HasIndex(x => new { x.Name, x.IsActive })
            .HasDatabaseName("idx_hspackage_name_active");

        builder.HasIndex(x => x.LastSync)
            .HasDatabaseName("idx_hspackage_last_sync");

        // índice por auditoría
        builder.HasIndex(x => x.CreatedById)
            .HasDatabaseName("idx_hspackage_created_by");

        builder.HasIndex(x => x.UpdatedById)
            .HasDatabaseName("idx_hspackage_updated_by");
    }
}