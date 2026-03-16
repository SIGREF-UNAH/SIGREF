using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Billing;

[Table("invoice_serie")]
public class InvoiceSerieEntity : BaseEntity
{
    // ===============================
    //         DATOS PRINCIPALES
    // ===============================

    [Required]
    [StringLength(150)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(10)]
    [Column("prefix")]
    public string Prefix { get; set; } = null!;

    // ===============================
    //       RANGO DE NUMERACIÓN
    // ===============================

    [Required]
    [Column("start_number")]
    public long StartNumber { get; set; }

    [Required]
    [Column("end_number")]
    public long EndNumber { get; set; }

    [Required]
    [Column("current_number")]
    public long CurrentNumber { get; set; }
    
}