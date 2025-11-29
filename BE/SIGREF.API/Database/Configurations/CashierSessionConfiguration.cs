using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Cashier;

namespace SIGREF.API.Database.Configurations;

public class CashierSessionConfiguration : IEntityTypeConfiguration<CashierSessionEntity>
{
    public void Configure(EntityTypeBuilder<CashierSessionEntity> builder)
    {
        // ============================
        //            TABLE
        // ============================
        builder.ToTable("cashier_sessions",
            t =>
            {
                t.HasComment(
                    "Tabla que almacena las sesiones de caja por usuario, incluyendo montos, diferencias y estado del arqueo.");
            });

        // ============================
        //          PRIMARY KEY
        // ============================
        builder.HasKey(x => x.Id);

        // ============================
        //          RELACIONES
        // ============================

        // Usuario (ID de Keycloak)
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnName("user_id")
            .HasComment("Id del usuario (Keycloak) al que pertenece la sesión de caja.");

        // Turno (Shift)
        builder.HasOne(x => x.Shift)
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ShiftId)
            .IsRequired()
            .HasColumnName("shift_id")
            .HasComment("Turno asignado a esta sesión de caja.");

        // ============================
        //        PROPIEDADES
        // ============================

        builder.Property(x => x.OpenAt)
            .IsRequired()
            .HasColumnName("open_at")
            .HasComment("Fecha y hora exacta en la que el cajero abrió la sesión de caja.");

        builder.Property(x => x.ClosedAt)
            .HasColumnName("closed_at")
            .HasComment("Fecha y hora en la que se cerró la sesión de caja.");

        builder.Property(x => x.DeclaredAmount)
            .HasColumnName("declared_amount")
            .HasPrecision(14, 2)
            .HasComment("Monto declarado por el cajero al momento del cierre.");

        builder.Property(x => x.SystemAmount)
            .HasColumnName("system_amount")
            .HasPrecision(14, 2)
            .HasComment("Monto calculado automáticamente por el sistema según los recibos generados.");

        builder.Property(x => x.Difference)
            .HasColumnName("difference")
            .HasPrecision(14, 2)
            .HasComment("Diferencia entre el monto declarado por el cajero y el monto calculado por el sistema.");

        builder.Property(x => x.IsOpen)
            .HasColumnName("is_open")
            .IsRequired()
            .HasComment("Indica si la sesión está activa (abierta).");

        builder.Property(x => x.RequiresCorrection)
            .HasColumnName("requires_correction")
            .IsRequired()
            .HasComment("Indica si la sesión requiere corrección debido a una diferencia detectada.");

        builder.Property(x => x.CorrectionDate)
            .HasColumnName("correction_date")
            .HasComment("Fecha en la que la sesión fue revisada/corregida por un administrador o auditor.");

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500)
            .HasComment("Notas o comentarios del cajero o administrador sobre discrepancias o correcciones.");

        // ============================
        //            INDEXES
        // ============================

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("idx_cashier_sessions_user");

        builder.HasIndex(x => x.ShiftId)
            .HasDatabaseName("idx_cashier_sessions_shift");

        builder.HasIndex(x => x.IsOpen)
            .HasDatabaseName("idx_cashier_sessions_is_open");

        builder.HasIndex(x => x.RequiresCorrection)
            .HasDatabaseName("idx_cashier_sessions_requires_correction");

        builder.HasIndex(x => x.OpenAt)
            .HasDatabaseName("idx_cashier_sessions_open_at");

        builder.HasIndex(x => x.ClosedAt)
            .HasDatabaseName("idx_cashier_sessions_closed_at");

        // builder.HasIndex(x => x.Difference)
        //     .HasDatabaseName("idx_cashier_sessions_difference");

        // UNIQUE para que un usuario no tenga 2 sesiones abiertas
        builder.HasIndex(x => new { x.UserId, x.IsOpen })
            .IsUnique()
            .HasDatabaseName("uq_cashier_sessions_user_open");
    }
}