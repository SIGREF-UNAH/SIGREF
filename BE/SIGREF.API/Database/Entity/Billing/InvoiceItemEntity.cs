using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Catalogs;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Billing;

[Table("invoice_items")]
public class InvoiceItemEntity : BaseEntity
{
    // ===============================
    //            FK FACTURA
    // ===============================

    [Required]
    [Column("invoice_id")]
    public Guid InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public InvoiceEntity? Invoice { get; set; }

    // ===============================
    //     FK SERVICIO / PAQUETE
    // ===============================

    [Column("service_id")]
    public Guid? ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public HealthService? Service { get; set; }

    [Column("package_id")]
    public Guid? PackageId { get; set; }

    [ForeignKey(nameof(PackageId))]
    public HealthServicePackagesEntity? Package { get; set; }

    // ===============================
    //        DATOS DEL ÍTEM
    // ===============================

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("discount")]
    public decimal? Discount { get; set; }
    
}