using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Cashier;
// crearTurno
// ui nos envia  location_id y propiedades del turno
// vallidar  ue la location exista en fhir y esté habilitada (opcional)
// se manda a crear la entidad a nuestra db

[Table("shifts")]
public class ShiftEntity : BaseEntity
{
    // ======================================
    //             RELACIÓN
    // ======================================

    [Required]
    [Column("location_id")]
    [StringLength(64)]
    public string LocationId { get; set; }
 


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

    [Column("correction_closure")]
    public DateTime CorrectionClosure { get; set; }
}