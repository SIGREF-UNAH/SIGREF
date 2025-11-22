using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Database.Entity.Administration;

[Table("hospital_properties")]
public class HospitalPropertiesEntity : BaseEntity
{
    // ===============================
    //        DATOS GENERALES
    // ===============================

    [Required]
    [StringLength(200)]
    [Column("name")]
    public string Name { get; set; } = null!;

    [StringLength(150)]
    [Column("director")]
    public string? Director { get; set; }

    [StringLength(150)]
    [Column("subdirector")]
    public string? Subdirector { get; set; }

    [StringLength(300)]
    [Column("location")]
    public string? Ubication { get; set; }

    // ===============================
    //       IMÁGENES / LOGOS
    // ===============================

    [StringLength(300)]
    [Column("url_logo")]
    public string? UrlLogo { get; set; }

    [StringLength(300)]
    [Column("url_logo_health")]
    public string? UrlLogoHealth { get; set; }

    // ===============================
    //       CONTACTO
    // ===============================

    [StringLength(20)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    [EmailAddress]
    [Column("email")]
    public string? Email { get; set; }

    // ===============================
    //       DATOS ADICIONALES
    // ===============================

    [StringLength(20)]
    [Column("hospital_code")]
    public string? HospitalCode { get; set; } // Código identificador interno

    [StringLength(50)] [Column("rtn")] public string? RTN { get; set; } // Identificación fiscal (si aplica)

    [StringLength(200)]
    [Url]
    [Column("website")]
    public string? Website { get; set; }

    [StringLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "LPS"; // Moneda del sistema

    [StringLength(10)]
    [Column("exchange_version")]
    public string? ExchangeVersion { get; set; } // Versión del tipo de cambio usado

    // ===============================
    //      CONTROL DEL SISTEMA
    // ===============================

    [Column("is_singleton")] public bool IsSingleton { get; set; } = true;
    // Útil para controlar que solo exista un registro
}