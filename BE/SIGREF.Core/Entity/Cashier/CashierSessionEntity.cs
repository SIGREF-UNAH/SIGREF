using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Cashier;

[Table("cashier_sessions")]
public class CashierSessionEntity : BaseEntity
{
    // ======================================
    //             RELACIONES
    // ======================================

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Required]
    [Column("shift_id")]
    public Guid ShiftId { get; set; }

    [ForeignKey(nameof(ShiftId))]
    public ShiftEntity? Shift { get; set; }


    // ======================================
    //        DATOS PRINCIPALES
    // ======================================

    [Required]
    [Column("open_at")]
    public DateTime OpenAt { get; set; }

    [Column("closed_at")]
    public DateTime? ClosedAt { get; set; }


    // ======================================
    //           MONTOS / ARQUEO
    // ======================================

    [Column("declared_amount")]
    public decimal? DeclaredAmount { get; set; }

    [Column("system_amount")]
    public decimal? SystemAmount { get; set; }

    [Column("difference")]
    public decimal? Difference { get; set; }

    // ======================================
    //         ESTADO DEL TURNO
    // ======================================

    [Column("is_open")]
    public bool IsOpen { get; set; } = true;
    
    [Column("requires_correction")]
    public bool RequiresCorrection { get; set; }

    [Column("correction_date")]
    public DateTime? CorrectionDate { get; set; }
    // ======================================
    //               NOTAS
    // ======================================

    [StringLength(500)]
    [Column("notes")]
    public string? Notes { get; set; }
}