using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Cashier;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<ShiftEntity>
{
    public void Configure(EntityTypeBuilder<ShiftEntity> builder)
    {
        // ============================
        //        TABLE
        // ============================
        builder.ToTable("shifts",
            t => { t.HasComment("Catálogo de turnos asignados a ubicaciones del hospital (Location - FHIR)."); });

        // ============================
        //        PRIMARY KEY
        // ============================
        builder.HasKey(x => x.Id);

        // ============================
        //     PROPIEDADES PRINCIPALES
        // ============================

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .HasMaxLength(64)
            .IsRequired()
            .HasComment("ID de la Location en FHIR asociada a este turno.");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired()
            .HasComment("Nombre del turno (ejemplo: 'Matutino', 'Vespertino', etc.).");

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired()
            .HasComment("Hora de inicio del turno (TimeOnly).");

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .IsRequired()
            .HasComment("Hora de finalización del turno (TimeOnly).");

        // ============================
        //          ESTADO
        // ============================
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasComment("Indica si el turno está activo.");

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

        // ============================
        //          ÍNDICES
        // ============================

        builder.HasIndex(x => x.LocationId)
            .HasDatabaseName("idx_shifts_location");

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("idx_shifts_name");

        builder.HasIndex(x => new { x.Name, x.LocationId })
            .HasDatabaseName("idx_shifts_name_location");
        
    }
}