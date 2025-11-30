using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Reports;

namespace SIGREF.API.Database.Configurations;

public class ReportHistoryEntityConfiguration : IEntityTypeConfiguration<ReportHistoryEntity>
{
    public void Configure(EntityTypeBuilder<ReportHistoryEntity> builder)
    {
        // ===============================
        //           TABLE
        // ===============================
        builder.ToTable("report_history",
            t =>
            {
                t.HasComment(
                    "Historial de reportes ejecutados en SIGREF. Guarda SQL generado, tipo de reporte, usuario ejecutor, formato y snapshot del hospital.");
            });

        // ===============================
        //           PRIMARY KEY
        // ===============================
        builder.HasKey(e => e.Id);


        // ===============================
        //          PROPIEDADES
        // ===============================

        builder.Property(e => e.ReportType)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("report_type");

        builder.Property(e => e.SqlQuery)
            .IsRequired()
            .HasColumnName("sql_query");

        builder.Property(e => e.Display)
            .HasMaxLength(300)
            .HasColumnName("display");

        builder.Property(e => e.DisplaySystem)
            .HasMaxLength(300)
            .HasColumnName("system_display");

        builder.Property(e => e.Format)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("format");


        // =====================================
        //  SNAPSHOT DEL HOSPITAL (SIN FK)
        // =====================================

        builder.Property(e => e.HospitalPropertiesSnapshot)
            .IsRequired()
            .HasColumnName("hospital_properties_snapshot");


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
        //             ÍNDICES
        // ===============================

        builder.HasIndex(e => e.ReportType)
            .HasDatabaseName("idx_report_history_type");

        builder.HasIndex(e => e.CreatedByUserId)
            .HasDatabaseName("idx_report_history_user");
    }
}