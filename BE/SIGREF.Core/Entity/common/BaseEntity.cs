using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGREF.Core.Entity.common;

public abstract class BaseEntity
{
    // ============================
    //        PRIMARY KEY
    // ============================
    public Guid Id { get; set; } = Guid.NewGuid();
    // ============================
    //          ESTADO
    // ============================
    public bool IsActive { get; set; } = true;
    // ============================
    //          AUDITORÍA
    // ============================
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
