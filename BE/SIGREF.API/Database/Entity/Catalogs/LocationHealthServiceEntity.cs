using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("locations_health_services")]
public class LocationHealthServiceEntity : BaseEntity
{
    // ======================================
    //           RELACIONES
    // ======================================

    [Required]
    [Column("location_id")]
    public Guid LocationId { get; set; }

    [Required]
    [Column("service_id")]
    public Guid ServiceId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public LocationEntity? Location { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public HealthService? Service { get; set; }
}