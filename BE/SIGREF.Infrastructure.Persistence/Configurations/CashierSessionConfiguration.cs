using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Cashier;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class CashierSessionConfiguration : BaseEntityConfiguration<CashierSessionEntity>
{
    public override void Configure(EntityTypeBuilder<CashierSessionEntity> builder)
    {
        // Llama a la configuración base
        base.Configure(builder);
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
            .HasConstraintName("fk_shift_id")
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
            .HasPrecision(18, 2)
            .HasComment("Monto declarado por el cajero al momento del cierre.");

        builder.Property(x => x.SystemAmount)
            .HasColumnName("system_amount")
            .HasPrecision(18, 2)
            .HasComment("Monto calculado automáticamente por el sistema según los recibos generados.");

        builder.Property(x => x.Difference)
            .HasColumnName("difference")
            .HasPrecision(18, 2)
            .HasComment("Diferencia entre el monto declarado por el cajero y el monto calculado por el sistema.");

        builder.Property(x => x.IsOpen)
            .IsRequired()
            .HasColumnName("is_open")
            .HasComment("Indica si la sesión está activa (abierta).")
            .HasDefaultValue(true);

        builder.Property(x => x.RequiresCorrection)
            .IsRequired()
            .HasColumnName("requires_correction")
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

        // -----------------------------------------------------------------------------
        // EN: PostgreSQL + EF Core indexing note
        // This entity requires two different indexes on the same column (user_id):
        //   1) A UNIQUE PARTIAL index (WHERE is_open = true) to enforce the business
        //      rule: "a user can only have one open cashier session at a time".
        //   2) A standard (non-unique) index on (user_id) to optimize historical queries
        //      over closed sessions.
        //
        // Due to EF Core model snapshot limitations, defining multiple indexes with the
        // same key columns (user_id) may result in one index being overwritten/merged
        // during migrations, especially when a filter (partial index) is involved.
        // For this reason, we keep ONLY the partial unique index defined in the model,
        // and create the non-unique index via a dedicated SQL migration.
        //
        // Suggested migration name:
        //   2026_01_30_AddIdxCashierSessionsUserSql
        //
        // ES: Nota sobre índices en PostgreSQL + EF Core
        // Esta entidad requiere dos índices distintos sobre la misma columna (user_id):
        //   1) Un índice ÚNICO PARCIAL (WHERE is_open = true) para garantizar la regla
        //      de negocio: "un usuario solo puede tener una sesión de caja abierta".
        //   2) Un índice normal (no único) sobre (user_id) para optimizar consultas
        //      históricas sobre sesiones cerradas.
        //
        // Debido a limitaciones del snapshot del modelo de EF Core, al definir múltiples
        // índices con las mismas columnas clave (user_id) puede ocurrir que uno sea
        // sobrescrito/colapsado durante la generación de migraciones, especialmente si
        // existe un filtro (índice parcial).
        // Por esa razón, aquí dejamos únicamente el índice parcial único en el modelo,
        // y el índice no-único se crea mediante una migración SQL dedicada.
        //
        // Nombre sugerido de migración:
        //   2026_01_30_AddIdxCashierSessionsUserSql
        // -----------------------------------------------------------------------------

        // builder.HasIndex(x => x.UserId)
        //     .HasDatabaseName("idx_cashier_sessions_user");

        // UNIQUE partial: a user cannot have 2 open sessions
    builder.HasIndex(x => x.UserId)
        .IsUnique()
        .HasDatabaseName("uq_cashier_sessions_user_open")
        .HasFilter("is_open = true");

    }
}