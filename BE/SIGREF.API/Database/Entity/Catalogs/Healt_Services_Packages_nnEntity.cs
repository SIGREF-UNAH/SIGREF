using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Catalogs;

[Table("health_services_packages_nn")]
public class HealthServicesPackagesNnEntity: BaseEntity
{
    // ======================================
    //        RELACIONES N:N
    // ======================================

    [Required(ErrorMessage = "El ID del servicio de salud es obligatorio.")]
    [Column("health_service_id")]
    public Guid HealthServiceId { get; set; }

    [Required(ErrorMessage = "El ID del paquete es obligatorio.")]
    [Column("package_id")]
    public Guid PackageId { get; set; }

    [ForeignKey(nameof(HealthServiceId))]
    public HealthService? HealthService { get; set; }

    [ForeignKey(nameof(PackageId))]
    public HealthServicePackagesEntity? Package { get; set; }
}

