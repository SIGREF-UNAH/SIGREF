using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Reports;

namespace SIGREF.Infrastructure.Persistence.Configurations;

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
        builder.Property(x => x.HangfireJobId)
            .HasMaxLength(100);
        
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.SqlQuery)
            .IsRequired()
            .HasColumnName("sql_query");

        builder.Property(x => x.DownloadUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PeriodLabel)
            .HasMaxLength(200);

        builder.Property(x => x.HospitalPropertiesSnapshot)
            .IsRequired()
            .HasColumnType("jsonb"); // ¡Punto de Senior! PSQL maneja JSONB de forma eficiente para búsquedas.

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("text");
        
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

        builder.Property(x => x.FilterJson)
            .HasColumnName("filter_json")
            .HasColumnType("jsonb")
            .HasComment("JSON Filter del reporte del hospital.");


        // ===============================
        //             ÍNDICES
        // ===============================

        builder.HasIndex(e => e.ReportType)
            .HasDatabaseName("idx_report_history_type");

        builder.HasIndex(e => e.CreatedById)
            .HasDatabaseName("idx_report_history_user");
    }
}