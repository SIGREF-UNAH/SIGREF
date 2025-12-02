using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.API.Database.Entity.Billing;

namespace SIGREF.API.Database.Configurations;

public class InvoiceEntityConfiguration : IEntityTypeConfiguration<InvoiceEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        builder.ToTable(
            "invoices",
            t => t.HasComment("Tabla principal de facturación: contiene facturas normales, emergencias, exentas y notas de crédito/débito.")
        );

        // ============================
        // PRIMARY KEY
        // ============================
        builder.HasKey(i => i.Id);

        // ============================
        // ENUMS (store as varchar)
        // ============================
        builder
            .Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("status")
            .HasComment("Created: Creada | Paid: pagada | Cancelled: anulada | Refunded: reembolsada");


        builder
            .Property(i => i.InvoiceType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("invoice_type")
            .HasComment("Normal: Todos Datos | Emergency: Se reconoce Servicio Dado Datos pueden quedar pendientes | Exempt: Descuento del 100% | Refunded: reembolsada | CreditNote: Devolucion de Dinero | DebitNote: Ingreso de Dinero");

        // ============================
        // PATIENT FIELDS
        // ============================
        builder.Property(i => i.PatientIdFhir).HasMaxLength(64);
        builder.Property(i => i.PatientDisplay).HasMaxLength(200);
        builder.Property(i => i.PatientSystem).HasMaxLength(50);
        builder.Property(i => i.PatientValue).HasMaxLength(200);

        // ============================
        // RELACIÓN: Invoice Items
        // ============================
        builder
            .HasMany(i => i.Items)
            .WithOne(i => i.Invoice)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ============================
        // RELACIÓN: Serie
        // ============================
        builder
            .HasOne(i => i.Serie)
            .WithMany()
            .HasForeignKey(i => i.SerieId)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================
        // RELACIÓN: ParentInvoice (ajustes)
        // ============================
        builder
            .HasOne(i => i.ParentInvoice)
            .WithMany()
            .HasForeignKey(i => i.ParentInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // ============================
        // RELACIÓN: CashierSession
        // ============================
        builder
            .HasOne(i => i.CashierSession)
            .WithMany()
            .HasForeignKey(i => i.CashierSessionId)
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

        // Index para filtros por fecha 
        builder.HasIndex(i => i.CreatedDate)
            .HasDatabaseName("idx_invoice_created_date");

        // ============================
        // PRECALCULATED TOTALS 
        // ============================

        builder.Property(i => i.TotalOriginal).HasPrecision(14, 2);
        builder.Property(i => i.AdjustmentTotal).HasPrecision(14, 2);
        builder.Property(i => i.FinalTotal).HasPrecision(14, 2);
        builder.Property(i => i.AmountPaid).HasPrecision(14, 2);
        builder.Property(i => i.AmountDue).HasPrecision(14, 2);

        // ============================
        // SERVICE / GROUP FIELDS
        // ============================
        builder.Property(i => i.ServiceGroupFhirId).HasMaxLength(64);
    }
}