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
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedDate { get; set; }
}