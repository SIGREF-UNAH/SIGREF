using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.Core.Entity.common;

namespace SIGREF.Core.Entity.Cashier;

public class CashierSessionEntity : BaseEntity
{
    // ======================================
    //             RELACIONES
    // ======================================
    public Guid UserId { get; set; }
    public Guid ShiftId { get; set; }
    public ShiftEntity? Shift { get; set; }
    // ======================================
    //        DATOS PRINCIPALES
    // ======================================
    public DateTimeOffset OpenAt { get; set; } =  DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }


    // ======================================
    //           MONTOS / ARQUEO
    // ======================================
    public decimal? DeclaredAmount { get; set; }
    public decimal? SystemAmount { get; set; }
    public decimal? Difference { get; set; }

    // ======================================
    //         ESTADO DEL TURNO
    // ======================================
    public bool IsOpen { get; set; } = true;
    public bool RequiresCorrection { get; set; }
    public DateTimeOffset? CorrectionDate { get; set; }
    // ======================================
    //               NOTAS
    // ======================================
    public string? Notes { get; set; }
}