using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Reports;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class ReportHistoryEntityConfiguration : BaseEntityConfiguration<ReportHistoryEntity>
{
    public override void Configure(EntityTypeBuilder<ReportHistoryEntity> builder)
    {
        base.Configure(builder);
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
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasColumnName("status")
            .HasComment("Estado del Reporte : Pending | Processing | Completed | Failed ");

        builder.Property(e => e.SqlQuery)
            .IsRequired()
            .HasColumnName("sql_query");

        builder.Property(x => x.DownloadUrl)
            .HasMaxLength(500)
            .HasColumnName("download_url");

        builder.Property(x => x.PeriodLabel)
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnName("period_label");

        builder.Property(x => x.HospitalPropertiesSnapshot)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasColumnName("hospital_properties_configuration");

        builder.Property(x => x.ErrorMessage)
            .HasColumnType("text")
            .HasColumnName("error_message");


        builder.Property(x => x.FilterJson)
            .HasColumnName("filter_json")
            .HasColumnType("jsonb")
            .HasColumnName("filter")
            .HasComment("JSON Filter del reporte del hospital.");


        // OTRHERS
        // TODO : Verificar si es de Tipo Required
        builder.Property(e => e.RequestedByUserId)
            .HasColumnName("requested_by_user_id");

        builder.Property(e => e.HangfireJobId)
            .HasMaxLength(50)
            .HasColumnName("hangfire_job_id");

        builder.Property(e => e.Progress)
            .HasMaxLength(3)
            .HasColumnName("progress");


        // ===============================
        //             ÍNDICES
        // ===============================

        builder.HasIndex(e => e.ReportType)
            .HasDatabaseName("idx_report_history_type");

        builder.HasIndex(e => e.CreatedById)
            .HasDatabaseName("idx_report_history_user");
    }
}