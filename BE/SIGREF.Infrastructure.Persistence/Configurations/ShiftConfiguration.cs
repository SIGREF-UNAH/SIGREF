using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Cashier;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : BaseEntityConfiguration<ShiftEntity>
{
    public override void Configure(EntityTypeBuilder<ShiftEntity> builder)
    {
        base.Configure(builder);
        // ============================
        //        TABLE
        // ============================
        builder.ToTable("shifts",
            t => { t.HasComment("Catálogo de turnos asignados a ubicaciones del hospital (Location - FHIR)."); });


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

        builder.Property(x => x.CorrectionClosure)
            .HasColumnName("correction_closure");

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