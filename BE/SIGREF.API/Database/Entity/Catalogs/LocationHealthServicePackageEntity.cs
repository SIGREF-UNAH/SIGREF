using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.Administration;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("locations_health_services_package")]
public class LocationHealthServicePackageEntity : BaseEntity
{
    // ===============================
    //           RELACIONES
    // ===============================

    [Required]
    [Column("location_id")]
    public Guid LocationId { get; set; }

    [Required]
    [Column("package_id")]
    public Guid PackageId { get; set; }
    

    [ForeignKey(nameof(PackageId))]
    public HealthServicePackagesEntity? Package { get; set; }
}