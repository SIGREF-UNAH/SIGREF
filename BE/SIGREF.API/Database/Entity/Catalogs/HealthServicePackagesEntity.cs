using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("health_services_package")]
public class HealthServicePackagesEntity : BaseEntity
{
    // ===============================
    //        DATOS PRINCIPALES
    // ===============================

    [Required(ErrorMessage = "El nombre del paquete es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
    [Column("name")]
    public string Name { get; set; } = null!;

    [StringLength(300, ErrorMessage = "La descripción no puede exceder los 300 caracteres.")]
    [Column("description")]
    public string? Description { get; set; }

    [StringLength(5, ErrorMessage = "La abreviación no puede exceder los 5 caracteres.")]
    [Column("abbreviation")]
    public string? Abbreviation { get; set; }

    [Required(ErrorMessage = "El precio del paquete es obligatorio.")]
    [Column("price")]
    public decimal Price { get; set; }

    // ===============================
    //        SINCRONIZACIÓN
    // ===============================

    [Column("last_sync")]
    public DateTime? LastSync { get; set; }
}
