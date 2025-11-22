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


        // =====================================
        //         USUARIO CREADOR
        // =====================================

        builder.Property(e => e.CreatedByUserId)
            .IsRequired()
            .HasColumnName("created_by_user_id");

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // ===============================
        //             ÍNDICES
        // ===============================

        builder.HasIndex(e => e.ReportType)
            .HasDatabaseName("idx_report_history_type");

        builder.HasIndex(e => e.CreatedByUserId)
            .HasDatabaseName("idx_report_history_user");
    }
}