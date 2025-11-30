using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Billing;

namespace SIGREF.API.Database.Configurations;

public class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        // ===============================
        //            TABLE
        // ===============================
        builder.ToTable("invoices",
            t =>
            {
                t.HasComment(
                    "Ordenes de Donacion emitidas por SIGREF, con información FHIR del paciente, series, métodos de pago y relaciones administrativas.");
            });


        // ===============================
        //           PRIMARY KEY
        // ===============================
        builder.HasKey(e => e.Id);


        // ===============================
        //        PROPIEDADES FHIR
        // ===============================

        builder.Property(e => e.PatientIdFhir)
            .HasMaxLength(64)
            .IsRequired()
            .HasColumnName("patient_id_fhir");

        builder.Property(e => e.PatientDisplay)
            .HasMaxLength(200)
            .HasColumnName("patient_display");

        builder.Property(e => e.PatientSystem)
            .HasMaxLength(50)
            .HasColumnName("patient_system");

        builder.Property(e => e.PatientValue)
            .HasMaxLength(200)
            .HasColumnName("patient_value");


        // ===============================
        //      USUARIO (CAJERO)
        // ===============================

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");
        


        // ===============================
        //       SERIE DE FACTURACIÓN
        // ===============================

        builder.Property(e => e.SerieId)
            .IsRequired()
            .HasColumnName("serie_id");

        builder.HasOne(e => e.Serie)
            .WithMany()
            .HasForeignKey(e => e.SerieId)
            .OnDelete(DeleteBehavior.Restrict);


        // ===============================
        //        DATOS DE FACTURA
        // ===============================

        builder.Property(e => e.Number)
            .IsRequired()
            .HasColumnName("number");

        builder.Property(e => e.TotalAmount)
            .IsRequired()
            .HasColumnName("total_amount");

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .IsRequired()
            .HasColumnName("currency");

        builder.Property(e => e.Status)
            .HasMaxLength(30)
            .IsRequired()
            .HasColumnName("status");


        // ===============================
        //        MÉTODO DE PAGO (ENUM)
        // ===============================

        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired()
            .HasColumnName("payment_method");


        // ===============================
        //        TIPO DE FACTURA (ENUM)
        // ===============================

        builder.Property(e => e.InvoiceType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnName("invoice_type");


        // ===============================
        //     FACTURA PADRE (RELACIÓN)
        // ===============================

        builder.Property(e => e.ParentInvoiceId)
            .HasColumnName("parent_invoice_id");

        builder.HasOne(e => e.ParentInvoice)
            .WithMany()
            .HasForeignKey(e => e.ParentInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);


        // ===============================
        //      SESIÓN DE CAJA (FK)
        // ===============================

        builder.Property(e => e.CashierSessionId)
            .HasColumnName("cashier_session_id");

        builder.HasOne(e => e.CashierSession)
            .WithMany()
            .HasForeignKey(e => e.CashierSessionId)
            .OnDelete(DeleteBehavior.Restrict);
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
        //           ÍNDICES
        // ===============================

        builder.HasIndex(e => e.Number)
            .HasDatabaseName("idx_invoice_number");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("idx_invoice_user");

        builder.HasIndex(e => e.SerieId)
            .HasDatabaseName("idx_invoice_serie");

        builder.HasIndex(e => e.CashierSessionId)
            .HasDatabaseName("idx_invoice_session");
    }
}