using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGREF.Core.Entity.common;

public abstract class BaseEntity
{
    // ============================
    //        PRIMARY KEY
    // ============================
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ============================
    //          ESTADO
    // ============================
    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // ============================
    //          AUDITORÍA
    // ============================
    [Required]
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }
    
    [Column("updated_by_id")]
    public Guid? UpdatedById { get; set; }
    
    [Required]
    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("updated_date")]
    public DateTime? UpdatedDate { get; set; }
}