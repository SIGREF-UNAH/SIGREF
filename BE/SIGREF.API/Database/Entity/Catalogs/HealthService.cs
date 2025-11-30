using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("health_services")]
public class HealthService : BaseEntity
{
    [Required]
    [StringLength(64)]
    [Column("health_service_id_fhir")]
    public string HealthServiceFhirId { get; set; }

    // ======================
    //   PRECIO
    // ======================
    [Required] [Column("price")] public decimal Price { get; set; }
}