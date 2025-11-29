using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("health_services")]
public class HealthServicesEntity:BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    // [Required]
    // [StringLength(150)]
    // [Column("name")]
    // public string Name { get; set; }

    // [StringLength(300)]
    // [Column("description")]
    // public string? Description { get; set; }

    [Required]
    [StringLength(64)]
    [Column("health_service_id_fhir")]
    public string HealthServiceIdFHIR { get; set; }

    // [StringLength(5)]
    // [Column("abbreviation")]
    // public string? Abbreviation { get; set; }

    // [Required]
    // [Column("is_active")]
    // public bool IsActive { get; set; }

    // ======================
    //   PRECIO
    // ======================
    [Required]
    [Column("price")]
    public decimal Price { get; set; }

    // ======================
    //   AUDITORÍA
    // ======================
    
// ======================
    //      AUDITORÍA
    // ======================

    [Required]
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public UserLinkEntity CreatedBy { get; set; }

    [Column("modified_by_id")]
    public Guid? ModifiedById { get; set; }

    [ForeignKey(nameof(ModifiedById))]
    public UserLinkEntity? ModifiedBy { get; set; }
    // ======================
    //  FECHAS
    // ======================
    
    [Required]
    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("updated_date")]
    public DateTime? UpdatedDate { get; set; } 
}