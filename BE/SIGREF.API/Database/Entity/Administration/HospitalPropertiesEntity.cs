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
    //       IMEGES / LOGOS
    // ===============================

    // ID del archivo en media_files
    [Column("logo_media_id")]
    public Guid? LogoMediaId { get; set; }

    // URL publica construida
    [StringLength(300)]
    [Column("url_logo")]
    public string? UrlLogo { get; set; }

    [Column("health_logo_media_id")]
    public Guid? HealthLogoMediaId { get; set; }

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
    public string? HospitalCode { get; set; }

    [StringLength(50)]
    [Column("rtn")]
    public string? RTN { get; set; }

    [StringLength(200)]
    [Url]
    [Column("website")]
    public string? Website { get; set; }

    [StringLength(10)]
    [Column("currency")]
    public string Currency { get; set; } = "LPS";

    // TODO : EL DIA QUE SE MANEJEN 2 TIPOS DE MONEDAS SERA NECESARIO ESTAR GUARDANDO CON EL TIPO DE CAMBIO UTILIZADO
    //[StringLength(10)]
    //[Column("exchange_version")]
    //public string? ExchangeVersion { get; set; }

    // ===============================
    //      CONTROL DEL SISTEMA
    // ===============================
    [Column("is_singleton")]
    public bool IsSingleton { get; set; } = true;
}