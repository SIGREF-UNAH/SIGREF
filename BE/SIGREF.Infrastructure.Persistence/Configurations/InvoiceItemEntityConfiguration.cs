using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class InvoiceItemEntityConfiguration : IEntityTypeConfiguration<InvoiceItemEntity>
{
    public void Configure(EntityTypeBuilder<InvoiceItemEntity> builder)
    {
        builder.ToTable(
            "invoice_items",
            t => t.HasComment("Items facturados: cada servicio congelado con precio histórico.")
        );

        // ============================
        // PRIMARY KEY
        // ============================
        builder.HasKey(i => i.Id);

        // ============================
        // RELACIÓN: Invoice - Items
        // ============================
        builder
            .HasOne(i => i.Invoice)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index FK obligatorio
        builder.HasIndex(i => i.InvoiceId)
            .HasDatabaseName("idx_invoiceitems_invoiceid");

        // Index compuesto opcional 
        builder.HasIndex(i => new { i.InvoiceId, i.ServiceId })
            .HasDatabaseName("idx_invoiceitems_invoiceid_serviceid");

        // ============================
        // RELACIÓN: Service - Items
        // ============================
        builder
            .HasOne(i => i.Service)
            .WithMany()
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index FK del servicio
        builder.HasIndex(i => i.ServiceId)
            .HasDatabaseName("idx_invoiceitems_serviceid");

        // ============================
        // PROPIEDADES
        // ============================
        builder.Property(i => i.Description)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nombre del servicio copiado al momento de facturar (histórico).");

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasComment("Cantidad facturada del servicio.");

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasComment("Precio unitario histórico del servicio facturado.");

        builder.Property(i => i.Discount)
            .HasPrecision(14, 2)
            .HasComment("Descuento aplicado al item (si aplica).");

        builder.Property(i => i.TotalAmount)
            .IsRequired()
            .HasPrecision(14, 2)
            .HasComment("Total del item: (quantity * unit_price) - discount (congelado).");

        // ============================
        // AUDITORÍA
        // ============================
        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired()
            .HasComment("Usuario que creó el item.");

        builder.Property(x => x.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasComment("Usuario que actualizó el item (si aplica).");

        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired()
            .HasComment("Fecha de creación del item (UTC).");

        builder.Property(x => x.UpdatedDate)
            .HasColumnName("updated_date")
            .HasComment("Fecha de última actualización del item (UTC).");
        
        builder.HasIndex(i => i.CreatedDate)
            .HasDatabaseName("idx_invoiceitems_created_date");
        builder.HasIndex(i => new { i.ServiceId, i.CreatedDate })
            .HasDatabaseName("idx_invoiceitems_serviceid_created");

    }
}