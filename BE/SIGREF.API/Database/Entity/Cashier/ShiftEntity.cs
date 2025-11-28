using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Cashier;

[Table("shifts")]
public class ShiftEntity : BaseEntity
{
    // ======================================
    //             RELACIÓN
    // ======================================

    [Required]
    [Column("location_id")]
    public Guid LocationId { get; set; }
    // ======================================
    //         INFORMACIÓN DEL TURNO
    // ======================================

    [Required]
    [StringLength(150)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [Required]
    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Required]
    [Column("end_time")]
    public TimeOnly EndTime { get; set; }
}