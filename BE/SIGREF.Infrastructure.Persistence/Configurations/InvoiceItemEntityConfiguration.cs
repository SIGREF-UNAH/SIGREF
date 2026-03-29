using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class InvoiceItemEntityConfiguration : BaseEntityConfiguration<InvoiceItemEntity>
{
    public override void Configure(EntityTypeBuilder<InvoiceItemEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable(
            "invoice_items",
            t => t.HasComment("Items facturados: cada servicio congelado con precio histórico.")
        );
        

        // ============================
        //          RELACIONES
        // ============================
        builder.HasOne(i => i.Invoice)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InvoiceId)
            .IsRequired()
            .HasForeignKey("fk_invoice_items_invoice")
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(i => i.InvoiceId)
            .HasColumnName("invoice_id") 
            .IsRequired();
        

        // ============================
        // RELACIÓN: Service - Items
        // ============================
        builder
            .HasOne(i => i.Service)
            .WithMany()
            .HasForeignKey(i => i.ServiceId)
            .HasConstraintName("fk_invoice_items_service")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.ServiceId)
            .HasColumnName("service_id") 
            .IsRequired();

        // ============================
        //         PROPIEDADES
        // ============================
        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nombre del servicio copiado al momento de facturar (congelado).");

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired()
            .HasComment("Cantidad facturada de este ítem.");

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .IsRequired()
            .HasPrecision(14, 2)
            .HasComment("Precio unitario histórico del servicio al momento de la venta.");

        builder.Property(i => i.TotalAmount)
            .HasColumnName("total_amount")
            .IsRequired()
            .HasPrecision(14, 2)
            .HasComment("Total de la línea: (Quantity * UnitPrice). No incluye descuentos.");

        
        builder.HasIndex(i => i.CreatedDate)
            .HasDatabaseName("idx_invoiceitems_created_date");
        builder.HasIndex(i => new { i.ServiceId, i.CreatedDate })
            .HasDatabaseName("idx_invoiceitems_serviceid_created");
        
        // Index FK obligatorio
        builder.HasIndex(i => i.InvoiceId)
            .HasDatabaseName("idx_invoiceitems_invoiceid");

        // Index compuesto opcional 
        builder.HasIndex(i => new { i.InvoiceId, i.ServiceId })
            .HasDatabaseName("idx_invoiceitems_invoiceid_serviceid");

        // Index FK del servicio
        builder.HasIndex(i => i.ServiceId)
            .HasDatabaseName("idx_invoiceitems_serviceid");

    }
}