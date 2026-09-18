using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class InvoiceEntityConfiguration : BaseEntityConfiguration<InvoiceEntity>
{
    public override void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable(
            "invoices",
            t => t.HasComment(
                "Tabla principal de facturación: contiene facturas normales, emergencias, exentas y notas de crédito/débito.")
        );

        // ============================
        //        PATIENT DATA
        // ============================
        builder.Property(i => i.PatientIdFhir)
            .HasColumnName("patient_id_fhir")
            .HasMaxLength(64)
            .HasComment("ID único del paciente en el servidor externo FHIR.");

        builder.Property(i => i.PatientDisplay)
            .HasColumnName("patient_display")
            .HasMaxLength(200)
            .HasComment("Nombre o alias del paciente al momento de facturar.");

        builder.Property(i => i.PatientSystem)
            .HasColumnName("patient_system")
            .HasMaxLength(100)
            .HasComment("Namespace del sistema de identificación (ej: URL de identidad).");

        builder.Property(i => i.PatientValue)
            .HasColumnName("patient_value")
            .HasMaxLength(50)
            .HasComment("Valor del documento de identidad (DNI/Pasaporte).");


        // ============================
        //         ENUMERATIONS
        // ============================
        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("status")
            .HasComment("Created: Creada | Paid: Pagada | Cancelled: Anulada | Refunded: Reembolsada");

        builder.Property(i => i.InvoiceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("invoice_type")
            .HasComment("Tipo legal: Normal, Emergency, Exempt, CreditNote, DebitNote.");

        builder.Property(i => i.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("payment_method")
            .HasComment("Método de pago: Cash, Card, Transfer, Mixed.");

        // ============================
        //     FINANCIAL TOTALS
        // ============================
        builder.Property(i => i.TotalOriginal)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("total_original")
            .HasComment("Monto bruto total (Suma de items).");

        builder.Property(i => i.InvoiceDiscount)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("invoice_discount")
            .HasComment("Descuento total aplicado a la factura en su creación.");

        builder.Property(i => i.AdjustmentTotal)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("adjustment_total")
            .HasComment("Suma neta de ajustes por notas de crédito/débito.");

        builder.Property(i => i.FinalTotal)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("final_total")
            .HasComment("Total exigible (TotalOriginal - Discount + Adjustment).");

        builder.Property(i => i.AmountPaid)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("amount_paid")
            .HasComment("Monto efectivamente cobrado.");

        builder.Property(i => i.AmountDue)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasColumnName("amount_due")
            .HasComment("Monto pendiente de cobro.");
        // ============================
        // RELACIÓN: Invoice Items
        // ============================
        builder
            .HasMany(i => i.Items)
            .WithOne(i => i.Invoice)
            .HasForeignKey(i => i.InvoiceId)
            .HasConstraintName("fk_invoice_items_id")
            .OnDelete(DeleteBehavior.Cascade);

        // ============================
        // RELACIÓN: Serie
        // ============================
        builder
            .HasOne(i => i.Serie)
            .WithMany()
            .HasForeignKey(i => i.SerieId)
            .HasConstraintName("fk_invoice_serie_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.SerieId)
            .IsRequired()
            .HasColumnName("serie_id");
        // ============================
        // RELACIÓN: ParentInvoice (ajustes)
        // ============================
        builder
            .HasOne(i => i.ParentInvoice)
            .WithMany()
            .HasForeignKey(i => i.ParentInvoiceId)
            .HasConstraintName("fk_parent_invoice_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.ParentInvoiceId)
            .HasColumnName("parent_invoice_id");
        // ============================
        // RELACIÓN: CashierSession
        // ============================
        builder
            .HasOne(i => i.CashierSession)
            .WithMany()
            .HasForeignKey(i => i.CashierSessionId)
            .HasConstraintName("fk_cashier_session_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.CashierSessionId)
            .IsRequired()
            .HasColumnName("cashier_session_id");

        // ============================
        // INDEXES 
        // ============================

        builder.HasIndex(i => new { i.CreatedById })
            .HasDatabaseName("idx_invoice_created_by");

        builder.HasIndex(i => new { i.CashierSessionId })
            .HasDatabaseName("idx_invoice_cashier_sesion");

        builder.HasIndex(i => new { i.SerieId })
            .HasDatabaseName("idx_invoice_serie");

        // Index para buscar facturas por serie/número 
        builder.HasIndex(i => new { i.SerieId, i.Number })
            .HasDatabaseName("idx_invoice_serie_number");

        // Index por paciente
        builder.HasIndex(i => i.PatientIdFhir)
            .HasDatabaseName("idx_invoice_patient_id");

        // Index para velocidad de ajustes
        builder.HasIndex(i => i.ParentInvoiceId)
            .HasDatabaseName("idx_invoice_parent");

        // Index para filtro por tipo (normal/emergencia/exento/nota)
        builder.HasIndex(i => i.InvoiceType)
            .HasDatabaseName("idx_invoice_type");

        // Index para listar por estado (Created, Paid…)
        builder.HasIndex(i => i.Status)
            .HasDatabaseName("idx_invoice_status");

        builder.HasIndex(i => new { i.CreatedDate, i.Status, i.InvoiceType })
            .HasDatabaseName("idx_invoice_created_status_type");

        // Index para filtros por fecha 
        builder.HasIndex(i => i.CreatedDate)
            .HasDatabaseName("idx_invoice_created_date");


        // Metadata adicional
        builder.Property(i => i.ServiceGroupFhirId)
            .HasMaxLength(64)
            .HasColumnName("service_group_fhir_id");

        builder.Property(i => i.SingleServiceId)
            .HasColumnName("single_service_id");

        builder.Property(i => i.Number)
            .IsRequired()
            .HasColumnName("number");
    }
}