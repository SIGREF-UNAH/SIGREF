using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGREF.Core.Entity.Billing;

namespace SIGREF.Infrastructure.Persistence.Configurations;

public class InvoiceSerieEntityConfiguration : BaseEntityConfiguration<InvoiceSerieEntity>
{
    public override void Configure(EntityTypeBuilder<InvoiceSerieEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable(
            "invoice_serie",
            t => t.HasComment("Tabla para gestionar las series de facturación y el control de su correlativo actual.")
        );


        // ============================
        // PROPERTIES
        // ============================
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name")
            .HasComment("Nombre descriptivo de la serie (ej: Serie A - Principal)");

        builder.Property(i => i.Prefix)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("prefix")
            .HasComment("Prefijo de la factura (ej: F001, A)");

        builder.Property(i => i.StartNumber)
            .IsRequired()
            .HasColumnName("start_number")
            .HasComment("Número inicial autorizado para esta serie");

        builder.Property(i => i.EndNumber)
            .IsRequired()
            .HasColumnName("end_number")
            .HasComment("Número final autorizado para esta serie");

        builder.Property(i => i.CurrentNumber)
            .IsRequired()
            .HasColumnName("current_number")
            .HasComment("Último número de factura emitido en esta serie");
        
        // ============================
        // INDICES
        // ============================
        
        // El prefijo de una serie generalmente debe ser único para evitar colisiones 
        // en la generación de números de factura.
        builder.HasIndex(i => i.Prefix)
            .IsUnique()
            .HasDatabaseName("idx_invoice_serie_prefix");

        // Índice para búsquedas rápidas por nombre en el dashboard o UI
        builder.HasIndex(i => i.Name)
            .HasDatabaseName("idx_invoice_serie_name");
        
        
        
    }
}