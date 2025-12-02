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
    public InvoiceEntity Invoice { get; set; }

    // ===============================
    //     FK SERVICIO / PAQUETE
    // ===============================

    // Servicio individual facturado
    [Column("service_id")]
    public Guid ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public HealthService Service { get; set; }
    

    // ===============================
    //        DATOS DEL ÍTEM
    // ===============================

    /// <summary>
    /// Nombre del servicio copiado al momento de facturar.
    /// Esto congela la información histórica.
    /// </summary>
    [Required]
    [Column("description")]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("discount")]
    public decimal? Discount { get; set; }

    /// <summary>
    /// Total del ítem:
    /// (quantity * unit_price) - discount
    /// Congelado al momento de facturar y hacer los llamados mas facil
    /// </summary>
    [Required]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }
}